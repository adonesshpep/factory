using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/contractreviews")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ContractReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notifications;

        public ContractReviewsController(AppDbContext context, IAuditLogService auditLogService, INotificationService notification)
        {
            _context = context;
            _auditLogService = auditLogService;
            _notifications = notification;
        }

        [Authorize(Roles = "Admin, Sales")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateContractReviewDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);
            if (supplier is null || !supplier.IsActive)
                return BadRequest(new { message = "Supplier does not exist or is inactive" });

            var rfq = await _context.RFQs.FindAsync(dto.RFQId);
            if (rfq is null || !rfq.IsActive)
                return BadRequest(new { message = "RFQ does not exist or is inactive" });

            var creationDate = dto.CreationDate ?? DateTime.UtcNow;
            var expectedDate = dto.ExpectedDate!.Value;
            if (expectedDate.Date < creationDate.Date)
                return BadRequest(new { message = "Expected delivery date cannot be before the creation date" });

            var review = new ContractReview
            {
                Number = dto.Number.Trim(),
                CreationDate = creationDate,
                ExpectedDate = expectedDate,
                SupplierId = supplier.Id,
                RFQId = rfq.Id,
                ConnectionMethod = dto.ConnectionMethod ?? ConnectionMethods.Phone,
                Quantity = dto.Quantity!.Value,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try
            {
                _context.ContractReviews.Add(review);
                await _context.SaveChangesAsync();

                await _notifications.NotifyAsync(
                    "Accounting",
                    $"Contract review {review.Number} created; create a purchase order",
                    NotificationRelatedTypes.ContractReviewCreated,
                    relatedId: review.Id);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A contract review with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateContractReview",
                $"Created contract review '{review.Number}' (Id: {review.Id}, Supplier: {supplier.SupplierNumber}, RFQ: {rfq.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(review.Id));
        }

        [Authorize(Roles = "Admin, Sales")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateContractReviewDto dto)
        {
            var review = await _context.ContractReviews.FindAsync(id);
            if (review is null)
                return NotFound(new { message = "Contract review not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number))
                review.Number = dto.Number.Trim();

            if (dto.CreationDate.HasValue) review.CreationDate = dto.CreationDate.Value;
            if (dto.ExpectedDate.HasValue) review.ExpectedDate = dto.ExpectedDate.Value;

            if (review.ExpectedDate.Date < review.CreationDate.Date)
                return BadRequest(new { message = "Expected delivery date cannot be before the creation date" });

            if (dto.SupplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(dto.SupplierId.Value);
                if (supplier is null || !supplier.IsActive)
                    return BadRequest(new { message = "Supplier does not exist or is inactive" });
                review.SupplierId = supplier.Id;
            }

            if (dto.ConnectionMethod.HasValue)
                review.ConnectionMethod = dto.ConnectionMethod.Value;

            if (dto.RFQId.HasValue)
            {
                var rfq = await _context.RFQs.FindAsync(dto.RFQId.Value);
                if (rfq is null || !rfq.IsActive)
                    return BadRequest(new { message = "RFQ does not exist or is inactive" });
                review.RFQId = rfq.Id;
            }

            if (dto.Quantity.HasValue) review.Quantity = dto.Quantity.Value;
            if (dto.Status.HasValue) review.Status = dto.Status.Value;

            if (dto.Notes != null)
                review.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A contract review with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateContractReview",
                $"Updated contract review (Id: {review.Id}, Number: {review.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(review.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery]  CRStatus? status=null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ContractReviews
                .Include(c => c.Supplier)
                .Include(c => c.RFQ).ThenInclude(r => r.Material)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }
            query = query
                .OrderByDescending(c => c.CreationDate)
                .ThenByDescending(c => c.Id);
            var totalCount = await query.CountAsync();
            var reviews = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var fulfilled = await _context.GetFulfilledQuantitiesAsync(reviews.Select(r => r.Id));

            return Ok(new PagedResultDto<ContractReviewResponseDto>
            {
                Items = reviews.Select(r => ToDto(r, fulfilled.GetValueOrDefault(r.Id))).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var cr = await _context.ContractReviews.FindAsync(id);
            if (cr is null) return NotFound(new { message = "Contract Review not found" });
            return Ok(await ToResponseDto(id));
        }
        [Authorize(Roles = "Admin, Technical, Manager")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var review = await _context.ContractReviews.FindAsync(id);
            if (review is null)
                return NotFound(new { message = "Contract review not found" });

            review.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateContractReview" : "DeactivateContractReview",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} contract review '{review.Number}' (Id: {review.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Contract review {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.ContractReviews.FindAsync(id);
            if (review is null)
                return NotFound(new { message = "Contract review not found" });

            _context.ContractReviews.Remove(review);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(review);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteContractReview",
                $"Deleted contract review '{review.Number}' (Id: {review.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Contract review deleted successfully" });
        }

        private async Task<ContractReviewResponseDto> ToResponseDto(int id)
        {
            var review = await _context.ContractReviews
                .Include(c => c.Supplier)
                .Include(c => c.RFQ).ThenInclude(r => r.Material)
                .FirstAsync(c => c.Id == id);

            var fulfilled = await _context.GetFulfilledQuantitiesAsync(new[] { id });
            return ToDto(review, fulfilled.GetValueOrDefault(id));
        }

        private static ContractReviewResponseDto ToDto(ContractReview c, decimal fulfilled)
        {
            var (connectionAr, connectionEn) = c.ConnectionMethod.GetBilingualNames();
            var (statusAr, statusEn) = c.Status.GetBilingualNames();
            var (rfqStatusAr, rfqStatusEn) = c.RFQ.Status.GetBilingualNames();
            var m = c.RFQ.Material;

            return new ContractReviewResponseDto
            {
                Id = c.Id,
                Number = c.Number,
                CreationDate = c.CreationDate,
                ExpectedDate = c.ExpectedDate,

                SupplierId = c.SupplierId,
                SupplierNumber = c.Supplier.SupplierNumber,
                SupplierNameEn = c.Supplier.NameEn,
                SupplierNameAr = c.Supplier.NameAr,

                ConnectionMethod = c.ConnectionMethod,
                ConnectionMethodNameEn = connectionEn,
                ConnectionMethodNameAr = connectionAr,

                Status = c.Status,
                StatusNameEn = statusEn,
                StatusNameAr = statusAr,

                MaterialId = m.Id,
                MaterialNumber = m.MaterialNumber,
                MaterialNameEn = m.NameEn,
                MaterialNameAr = m.NameAr,

                RFQId = c.RFQId,
                RFQNumber = c.RFQ.Number,
                RFQStatus = c.RFQ.Status,
                RFQStatusNameEn = rfqStatusEn,
                RFQStatusNameAr = rfqStatusAr,
                RFQQuantity = c.RFQ.Quantity,
                // RFQ-level fulfilment totals are not computed here; leave as 0 until a dedicated helper exists.
                RFQFulfilledQuantity = 0,
                RFQRemainingQuantity = c.RFQ.Quantity,
                RFQFulfillmentPercentage = 0,
                RFQIsFulfilled = false,

                Quantity = c.Quantity,
                FulfilledQuantity = fulfilled,
                RemainingQuantity = Math.Max(0, c.Quantity - fulfilled),
                FulfillmentPercentage = ContractReviewFulfillment.Percentage(fulfilled, c.Quantity),
                IsFulfilled = fulfilled >= c.Quantity,

                Notes = c.Notes,
                IsActive = c.IsActive
            };
        }
    }
}