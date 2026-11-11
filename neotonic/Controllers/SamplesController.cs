using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/samples")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class SamplesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notifications;

        public SamplesController(AppDbContext context, IAuditLogService auditLogService, INotificationService notifications)
        {
            _context = context;
            _auditLogService = auditLogService;
            _notifications = notifications;
        }

        [Authorize(Roles = "Admin, Lab")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSampleDto dto)
        {
            var type = dto.SampleType!.Value;

            if (type == SampleTypes.Supplier)
            {
                if (dto.ContractReviewId is null)
                    return BadRequest(new { message = "ContractReviewId is required for a supplier sample" });
                if (dto.UomId is not null)
                    return BadRequest(new { message = "UomId must not be set for a supplier sample" });

                var review = await _context.ContractReviews.FindAsync(dto.ContractReviewId.Value);
                if (review is null || !review.IsActive)
                    return BadRequest(new { message = "Contract review does not exist or is inactive" });
            }
            else
            {
                if (dto.UomId is null)
                    return BadRequest(new { message = "UomId is required for a delivery sample" });
                if (dto.ContractReviewId is not null)
                    return BadRequest(new { message = "ContractReviewId must not be set for a delivery sample" });

                var uomExists = await _context.UOMs.AnyAsync(u => u.Id == dto.UomId.Value);
                if (!uomExists)
                    return BadRequest(new { message = "Box does not exist" });
            }

            var sample = new Sample
            {
                Number = dto.Number.Trim(),
                SampleType = type,
                SampleDate = dto.SampleDate!.Value,
                ContractReviewId = dto.ContractReviewId,
                UomId = dto.UomId,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Samples.Add(sample);
                await _context.SaveChangesAsync();  // sample.Id assigned

                if (type == SampleTypes.Delivery)
                {
                    await _notifications.NotifyAsync(
                        "Lab",
                        $"Sample {sample.Number} is ready; create a certificate",
                        NotificationRelatedTypes.DeliverySample,
                        relatedId: sample.Id);
                }

                await _context.SaveChangesAsync();  // commits the notification in the same transaction
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "A sample with this number already exists" });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            await _auditLogService.LogAsync(
                User,
                "CreateSample",
                $"Created {type} sample '{sample.Number}' (Id: {sample.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(sample.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? contractReviewId = null,
            [FromQuery] int? uomId = null,
            [FromQuery] SampleTypes? sampleType = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = QueryWithIncludes();

            if (contractReviewId.HasValue) query = query.Where(s => s.ContractReviewId == contractReviewId.Value);
            if (uomId.HasValue) query = query.Where(s => s.UomId == uomId.Value);
            if (sampleType.HasValue) query = query.Where(s => s.SampleType == sampleType.Value);

            var ordered = query.OrderByDescending(s => s.SampleDate).ThenByDescending(s => s.Id);
            var totalCount = await ordered.CountAsync();

            var samples = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new PagedResultDto<SampleResponseDto>
            {
                Items = samples.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var sample = await _context.Samples.FindAsync(id);
            if (sample is null) return NotFound(new { message = "Sample not found" });
            return Ok(await ToResponseDto(id));
        }
        [HttpGet("next-number")]
        public async Task<IActionResult> NextNumber([FromQuery] int? uomId = null)
        {
            if (!uomId.HasValue)
                return BadRequest(new { message = "uomId is required for a delivery sample number" });

            var uom = await _context.UOMs.FirstOrDefaultAsync(u => u.Id == uomId.Value);
            if (uom is null) return NotFound(new { message = "Box not found" });

            var prefix = uom.Number.ToString("D6");

            var existing = await _context.Samples
                .Where(s => s.SampleType == SampleTypes.Delivery && s.Number.StartsWith(prefix))
                .Select(s => s.Number)
                .ToListAsync();

            var maxSeq = 0;
            foreach (var number in existing)
            {
                var suffix = number.Substring(prefix.Length);
                if (int.TryParse(suffix, out var seq) && seq > maxSeq) maxSeq = seq;
            }

            var nextSeq = maxSeq + 1;
            if (nextSeq > 99)
                return Conflict(new { message = "This box already has 99 delivery samples; no further numbers available" });

            return Ok(new neotonic.DTOs.Responses.NextSampleNumberResponseDto
            {
                NextNumber = prefix + nextSeq.ToString("D2"),
                NextSequence = nextSeq,
                UomId = uom.Id,
                UomNumber = uom.Number
            });
        }
        [HttpGet("by-number/{number}")]
        public async Task<IActionResult> GetByNumber(string number)
        {
            var trimmed = number.Trim();
            var sample = await QueryWithIncludes().FirstOrDefaultAsync(s => s.Number == trimmed);
            if (sample is null)
                return NotFound(new { message = "Sample not found" });

            return Ok(ToDto(sample));
        }

        [Authorize(Roles = "Admin, Technical, Manager")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var sample = await _context.Samples.FindAsync(id);
            if (sample is null)
                return NotFound(new { message = "Sample not found" });

            sample.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateSample" : "DeactivateSample",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} sample '{sample.Number}' (Id: {sample.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Sample {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sample = await _context.Samples.FindAsync(id);
            if (sample is null)
                return NotFound(new { message = "Sample not found" });

            _context.Samples.Remove(sample);

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
                "DeleteSample",
                $"Deleted sample '{sample.Number}' (Id: {sample.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Sample deleted successfully" });
        }

        private IQueryable<Sample> QueryWithIncludes() =>
            _context.Samples
                .Include(s => s.ContractReview!).ThenInclude(cr => cr.Supplier)
                .Include(s => s.ContractReview!).ThenInclude(cr => cr.RFQ).ThenInclude(r => r.Material)
                .Include(s => s.Uom!).ThenInclude(u => u.Material);

        private async Task<SampleResponseDto> ToResponseDto(int id)
        {
            var sample = await QueryWithIncludes().FirstAsync(s => s.Id == id);
            return ToDto(sample);
        }

        private static SampleResponseDto ToDto(Sample s)
        {
            Supplier? supplier = null;
            ContractReview? review = null;
            int materialId; string materialNumber, materialEn, materialAr;

            if (s.SampleType == SampleTypes.Supplier)
            {
                review = s.ContractReview!;
                supplier = review.Supplier;
                var m = review.RFQ.Material;
                materialId = m.Id; materialNumber = m.MaterialNumber; materialEn = m.NameEn; materialAr = m.NameAr;
            }
            else
            {
                var m = s.Uom!.Material!;
                materialId = m.Id; materialNumber = m.MaterialNumber; materialEn = m.NameEn; materialAr = m.NameAr;
            }

            return new SampleResponseDto
            {
                Id = s.Id,
                Number = s.Number,
                SampleType = s.SampleType,
                SampleDate = s.SampleDate,

                SupplierId = supplier?.Id,
                SupplierNumber = supplier?.SupplierNumber,
                SupplierNameEn = supplier?.NameEn,
                SupplierNameAr = supplier?.NameAr,

                ContractReviewId = review?.Id,
                ContractReviewNumber = review?.Number,

                MaterialId = materialId,
                MaterialNumber = materialNumber,
                MaterialNameEn = materialEn,
                MaterialNameAr = materialAr,

                UomId = s.UomId,
                UomBarcode = s.Uom?.Barcode,

                Notes = s.Notes,
                IsActive = s.IsActive
            };
        }
    }
}