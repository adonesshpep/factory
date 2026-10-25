using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/suppliersamples")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class SupplierSamplesController : ControllerBase
    {
        //private const string SupplierMismatchMessage =
        //    "The supplier does not match the supplier of the selected contract review";

        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public SupplierSamplesController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSupplierSampleDto dto)
        {
            //var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);
            //if (supplier is null || !supplier.IsActive)
            //{
            //    return BadRequest(new { message = "Supplier does not exist or is inactive" });
            //}

            var review = await _context.ContractReviews.FindAsync(dto.ContractReviewId);
            if (review is null || !review.IsActive)
            {
                return BadRequest(new { message = "Contract review does not exist or is inactive" });
            }

            //if (review.SupplierId != supplier.Id)
            //{
            //    return BadRequest(new { message = SupplierMismatchMessage });
            //}

            var sample = new SupplierSample
            {
                Number = dto.Number.Trim(),
                DeliveryDate = dto.DeliveryDate!.Value,
                //SupplierId = supplier.Id,
                ContractReviewId = review.Id,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try
            {
                _context.SupplierSamples.Add(sample);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A supplier sample with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateSupplierSample",
                $"Created supplier sample '{sample.Number}' (Id: {sample.Id}, Contract review: {review.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(sample.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateSupplierSampleDto dto)
        {
            var sample = await _context.SupplierSamples.FindAsync(id);
            if (sample is null)
            {
                return NotFound(new { message = "Supplier sample not found" });
            }

            var oldNumber = sample.Number;

            if (!string.IsNullOrWhiteSpace(dto.Number))
                sample.Number = dto.Number.Trim();

            if (dto.DeliveryDate.HasValue)
                sample.DeliveryDate = dto.DeliveryDate.Value;

            if (dto.Notes != null)
                sample.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            //if (dto.SupplierId.HasValue)
            //{
            //    var supplier = await _context.Suppliers.FindAsync(dto.SupplierId.Value);
            //    if (supplier is null || !supplier.IsActive)
            //        return BadRequest(new { message = "Supplier does not exist or is inactive" });
            //    sample.SupplierId = supplier.Id;
            //}

            if (dto.ContractReviewId.HasValue)
            {
                var newReview = await _context.ContractReviews.FindAsync(dto.ContractReviewId.Value);
                if (newReview is null || !newReview.IsActive)
                    return BadRequest(new { message = "Contract review does not exist or is inactive" });
                sample.ContractReviewId = newReview.Id;
            }

            // If either link changed, the final supplier must still match the final review's supplier
            //if (dto.SupplierId.HasValue || dto.ContractReviewId.HasValue)
            //{
            //    var review = await _context.ContractReviews.FindAsync(sample.ContractReviewId);
            //    if (review!.SupplierId != sample.SupplierId)
            //        return BadRequest(new { message = SupplierMismatchMessage });
            //}

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A supplier sample with this number already exists" });
            }

            var numberNote = oldNumber != sample.Number
                ? $", Number changed from '{oldNumber}' to '{sample.Number}' (barcode label must be reprinted)"
                : string.Empty;

            await _auditLogService.LogAsync(
                User,
                "UpdateSupplierSample",
                $"Updated supplier sample (Id: {sample.Id}, Number: {sample.Number}){numberNote}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(sample.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? contractReviewId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = QueryWithIncludes();

            if (contractReviewId.HasValue)
                query = query.Where(s => s.ContractReviewId == contractReviewId.Value);

            var ordered = query
                .OrderByDescending(s => s.DeliveryDate)
                .ThenByDescending(s => s.Id);

            var totalCount = await ordered.CountAsync();

            var samples = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResultDto<SupplierSampleResponseDto>
            {
                Items = samples.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        // What a barcode scan calls: the scanner reads the number printed on the sample.
        [HttpGet("by-number/{number}")]
        public async Task<IActionResult> GetByNumber(string number)
        {
            var trimmed = number.Trim();
            var sample = await QueryWithIncludes().FirstOrDefaultAsync(s => s.Number == trimmed);
            if (sample is null)
            {
                return NotFound(new { message = "Supplier sample not found" });
            }

            return Ok(ToDto(sample));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var sample = await _context.SupplierSamples.FindAsync(id);
            if (sample is null)
            {
                return NotFound(new { message = "Supplier sample not found" });
            }

            sample.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateSupplierSample" : "DeactivateSupplierSample",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} supplier sample '{sample.Number}' (Id: {sample.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Supplier sample {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sample = await _context.SupplierSamples.FindAsync(id);
            if (sample is null)
            {
                return NotFound(new { message = "Supplier sample not found" });
            }

            _context.SupplierSamples.Remove(sample);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(sample);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteSupplierSample",
                $"Deleted supplier sample '{sample.Number}' (Id: {sample.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Supplier sample deleted successfully" });
        }

        private IQueryable<SupplierSample> QueryWithIncludes() =>
            _context.SupplierSamples
                //.Include(s => s.Supplier)
                .Include(s => s.ContractReview)
                    .ThenInclude(c => c.Material)
            .Include(c=>c.ContractReview)
                .ThenInclude(cr => cr.Supplier);

        private async Task<SupplierSampleResponseDto> ToResponseDto(int id)
        {
            var sample = await QueryWithIncludes().FirstAsync(s => s.Id == id);
            return ToDto(sample);
        }

        private static SupplierSampleResponseDto ToDto(SupplierSample s) => new()
        {
            Id = s.Id,
            Number = s.Number,
            DeliveryDate = s.DeliveryDate,
            SupplierId = s.SupplierId,
            SupplierNumber = s.ContractReview.Supplier.SupplierNumber,
            SupplierNameEn = s.ContractReview.Supplier.NameEn,
            SupplierNameAr = s.ContractReview.Supplier.NameAr,
            ContractReviewId = s.ContractReviewId,
            ContractReviewNumber = s.ContractReview.Number,
            MaterialId = s.ContractReview.MaterialId,
            MaterialNumber = s.ContractReview.Material.MaterialNumber,
            MaterialNameEn = s.ContractReview.Material.NameEn,
            MaterialNameAr = s.ContractReview.Material.NameAr,
            Notes = s.Notes,
            IsActive = s.IsActive
        };
    }
}