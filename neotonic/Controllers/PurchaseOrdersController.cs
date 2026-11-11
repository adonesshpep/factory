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
    [Route("api/purchaseorders")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class PurchaseOrdersController : ControllerBase
    {
        private const string DateMessage = "Delivery date must be after the creation date";

        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public PurchaseOrdersController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [Authorize(Roles = "Admin, Sales")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreatePurchaseOrderDto dto)
        {
            var linkError = await ValidateLinksAsync(dto.ContractReviewId, dto.CertificateOfAnalysisId);
            if (linkError is not null) return BadRequest(new { message = linkError });

            var creationDate = dto.CreationDate ?? DateTime.UtcNow;
            var deliveryDate = dto.DeliveryDate!.Value;
            if (deliveryDate.Date <= creationDate.Date)
                return BadRequest(new { message = DateMessage });

            var fulfillmentError = await CheckFulfillmentAsync(dto.ContractReviewId, dto.Quantity!.Value);
            if (fulfillmentError is not null) return BadRequest(new { message = fulfillmentError });

            var order = new PurchaseOrder
            {
                Number = dto.Number.Trim(),
                CreationDate = creationDate,
                ContractReviewId = dto.ContractReviewId,
                CertificateOfAnalysisId = dto.CertificateOfAnalysisId,
                Quantity = dto.Quantity.Value,
                DeliveryDate = deliveryDate,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try
            {
                _context.PurchaseOrders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A purchase order with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreatePurchaseOrder",
                $"Created purchase order '{order.Number}' (Id: {order.Id}, Contract review: {order.ContractReviewId}, Quantity: {order.Quantity})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [Authorize(Roles = "Admin, Sales")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdatePurchaseOrderDto dto)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order is null) return NotFound(new { message = "Purchase order not found" });

            var oldStatus = order.Status;

            if (!string.IsNullOrWhiteSpace(dto.Number)) order.Number = dto.Number.Trim();
            if (dto.CreationDate.HasValue) order.CreationDate = dto.CreationDate.Value;
            if (dto.DeliveryDate.HasValue) order.DeliveryDate = dto.DeliveryDate.Value;
            if (dto.ContractReviewId.HasValue) order.ContractReviewId = dto.ContractReviewId.Value;
            if (dto.CertificateOfAnalysisId.HasValue) order.CertificateOfAnalysisId = dto.CertificateOfAnalysisId.Value;
            if (dto.Quantity.HasValue) order.Quantity = dto.Quantity.Value;
            if (dto.Status.HasValue) order.Status = dto.Status.Value;
            if (dto.Notes != null)
                order.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (order.DeliveryDate.Date <= order.CreationDate.Date)
                return BadRequest(new { message = DateMessage });

            if (dto.ContractReviewId.HasValue || dto.CertificateOfAnalysisId.HasValue)
            {
                var linkError = await ValidateLinksAsync(order.ContractReviewId, order.CertificateOfAnalysisId);
                if (linkError is not null) return BadRequest(new { message = linkError });
            }

            var countsTowardReview = order.Status != POStatus.Cancelled;
            var comingBackFromCancelled = oldStatus == POStatus.Cancelled && countsTowardReview;

            if (countsTowardReview &&
                (dto.Quantity.HasValue || dto.ContractReviewId.HasValue || comingBackFromCancelled))
            {
                var fulfillmentError = await CheckFulfillmentAsync(order.ContractReviewId, order.Quantity, order.Id);
                if (fulfillmentError is not null) return BadRequest(new { message = fulfillmentError });
            }

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A purchase order with this number already exists" });
            }

            var statusNote = oldStatus != order.Status
                ? $", Status changed from {oldStatus} to {order.Status}"
                : string.Empty;

            await _auditLogService.LogAsync(
                User,
                "UpdatePurchaseOrder",
                $"Updated purchase order (Id: {order.Id}, Number: {order.Number}){statusNote}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
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
                query = query.Where(p => p.ContractReviewId == contractReviewId.Value);

            var ordered = query.OrderByDescending(p => p.CreationDate).ThenByDescending(p => p.Id);
            var totalCount = await ordered.CountAsync();
            var orders = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var fulfilled = await _context.GetFulfilledQuantitiesAsync(orders.Select(o => o.ContractReviewId));

            return Ok(new PagedResultDto<PurchaseOrderResponseDto>
            {
                Items = orders.Select(o => ToDto(o, fulfilled.GetValueOrDefault(o.ContractReviewId))).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var po = await _context.PurchaseOrders.FindAsync(id);
            if (po is null) return NotFound(new { message = "Purchase Order not found" });
            return Ok(await ToResponseDto(id));
        }
        [Authorize(Roles = "Admin, Technical, Manager")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order is null) return NotFound(new { message = "Purchase order not found" });

            order.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivatePurchaseOrder" : "DeactivatePurchaseOrder",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} purchase order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Purchase order {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order is null) return NotFound(new { message = "Purchase order not found" });

            _context.PurchaseOrders.Remove(order);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(order);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeletePurchaseOrder",
                $"Deleted purchase order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Purchase order deleted successfully" });
        }

        private async Task<string?> ValidateLinksAsync(int contractReviewId, int certificateId)
        {
            var review = await _context.ContractReviews.FindAsync(contractReviewId);
            if (review is null || !review.IsActive)
                return "Contract review does not exist or is inactive";

            var certificate = await _context.CertificatesOfAnalysis
                .Include(c => c.Sample)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            if (certificate is null || !certificate.IsActive)
                return "Certificate of analysis does not exist or is inactive";

            if (certificate.Sample.ContractReviewId != review.Id)
                return "The certificate does not belong to the selected contract review";

            // ---- PM DECISION PENDING (2): only allow accepted certificates ----
            // if (certificate.Result != AnalysisResults.Accepted)
            //     return "A purchase order can only be created from an accepted certificate";

            return null;
        }

        // ---- PM DECISION PENDING (1): over-fulfilment of the contract review ----
        private async Task<string?> CheckFulfillmentAsync(int contractReviewId, decimal quantity, int? excludePurchaseOrderId = null)
        {
            await Task.CompletedTask;
            // var review = await _context.ContractReviews.FindAsync(contractReviewId);
            // var fulfilled = (await _context.GetFulfilledQuantitiesAsync(new[] { contractReviewId }, excludePurchaseOrderId))
            //     .GetValueOrDefault(contractReviewId);
            // if (fulfilled >= review!.Quantity) return "This contract review is already fully covered by purchase orders";
            // if (fulfilled + quantity > review.Quantity)
            //     return $"Quantity exceeds the remaining {review.Quantity - fulfilled} of the contract review";
            return null;
        }

        private IQueryable<PurchaseOrder> QueryWithIncludes() =>
            _context.PurchaseOrders
                .Include(p => p.CertificateOfAnalysis).ThenInclude(c => c.Sample)
                .Include(p => p.ContractReview).ThenInclude(r => r.Supplier)
                .Include(p => p.ContractReview).ThenInclude(r => r.RFQ).ThenInclude(rfq => rfq.Material);

        private async Task<PurchaseOrderResponseDto> ToResponseDto(int id)
        {
            var order = await QueryWithIncludes().FirstAsync(p => p.Id == id);
            var fulfilled = await _context.GetFulfilledQuantitiesAsync(new[] { order.ContractReviewId });
            return ToDto(order, fulfilled.GetValueOrDefault(order.ContractReviewId));
        }

        private static PurchaseOrderResponseDto ToDto(PurchaseOrder p, decimal fulfilled)
        {
            var review = p.ContractReview;
            var rfq = review.RFQ;
            var m = rfq.Material;
            var certificate = p.CertificateOfAnalysis;
            var (statusAr, statusEn) = p.Status.GetBilingualNames();
            var (resultAr, resultEn) = certificate.Result.GetBilingualNames();

            return new PurchaseOrderResponseDto
            {
                Id = p.Id,
                Number = p.Number,
                CreationDate = p.CreationDate,
                DeliveryDate = p.DeliveryDate,
                Quantity = p.Quantity,

                Status = p.Status,
                StatusNameEn = statusEn,
                StatusNameAr = statusAr,

                CertificateOfAnalysisId = certificate.Id,
                CertificateNumber = certificate.Number,
                CertificateAnalysisDate = certificate.AnalysisDate,
                CertificateResult = certificate.Result,
                CertificateResultNameEn = resultEn,
                CertificateResultNameAr = resultAr,

                ContractReviewId = review.Id,
                ContractReviewNumber = review.Number,
                ContractReviewQuantity = review.Quantity,
                ContractReviewFulfilledQuantity = fulfilled,
                ContractReviewRemainingQuantity = Math.Max(0, review.Quantity - fulfilled),
                ContractReviewFulfillmentPercentage = ContractReviewFulfillment.Percentage(fulfilled, review.Quantity),
                ContractReviewIsFulfilled = fulfilled >= review.Quantity,

                SupplierId = review.SupplierId,
                SupplierNumber = review.Supplier.SupplierNumber,
                SupplierNameEn = review.Supplier.NameEn,
                SupplierNameAr = review.Supplier.NameAr,

                MaterialId = m.Id,
                MaterialNumber = m.MaterialNumber,
                MaterialNameEn = m.NameEn,
                MaterialNameAr = m.NameAr,

                Notes = p.Notes,
                IsActive = p.IsActive
            };
        }
    }
}