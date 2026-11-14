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
    [Route("api/rfqs")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class RFQController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notifications;

        public RFQController(AppDbContext context, IAuditLogService auditLogService, INotificationService notifications)
        {
            _context = context;
            _auditLogService = auditLogService;
            _notifications = notifications;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateRFQDto dto)
        {
            if (dto.AnnualPlanID.HasValue)
            {
                var plan = await _context.AnnualPlans.FindAsync(dto.AnnualPlanID.Value);
                if (plan is null || !plan.IsActive)
                    return BadRequest(new { message = "Annual plan does not exist or is inactive" });
            }

            var material = await _context.Materials.FindAsync(dto.MaterialId);
            if (material is null || !material.IsActive)
                return BadRequest(new { message = "Material does not exist or is inactive" });

            var rfq = new RFQ
            {
                Number = dto.Number.Trim(),
                AnnualPlanID = dto.AnnualPlanID,
                Status = dto.Status!.Value,
                MaterialId = material.Id,
                Quantity = dto.Quantity!.Value,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try {
                _context.RFQs.Add(rfq);
                await _context.SaveChangesAsync();

                await _notifications.NotifyAsync(
                    "Sales",
                    $"RFQ {rfq.Number} created; create a contract review",
                    NotificationRelatedTypes.RFQCreated,
                    relatedId: rfq.Id);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "An RFQ with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User, "CreateRFQ",
                $"Created RFQ '{rfq.Number}' (Id: {rfq.Id}, Material: {material.MaterialNumber}, Quantity: {rfq.Quantity})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(rfq.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateRFQDto dto)
        {
            var rfq = await _context.RFQs.FindAsync(id);
            if (rfq is null) return NotFound(new { message = "RFQ not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number)) rfq.Number = dto.Number.Trim();

            if (dto.AnnualPlanID.HasValue)
            {
                var plan = await _context.AnnualPlans.FindAsync(dto.AnnualPlanID.Value);
                if (plan is null || !plan.IsActive)
                    return BadRequest(new { message = "Annual plan does not exist or is inactive" });
                rfq.AnnualPlanID = plan.Id;
            }

            if (dto.Status.HasValue) rfq.Status = dto.Status.Value;

            if (dto.MaterialId.HasValue)
            {
                var material = await _context.Materials.FindAsync(dto.MaterialId.Value);
                if (material is null || !material.IsActive)
                    return BadRequest(new { message = "Material does not exist or is inactive" });
                rfq.MaterialId = material.Id;
            }

            if (dto.Quantity.HasValue) rfq.Quantity = dto.Quantity.Value;
            if (dto.Notes != null)
                rfq.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "An RFQ with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User, "UpdateRFQ",
                $"Updated RFQ (Id: {rfq.Id}, Number: {rfq.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(rfq.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 20,
     [FromQuery] RFQStatus? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.RFQs
                .Include(r => r.AnnualPlan)
                .Include(r => r.Material)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var ordered = query
                .OrderByDescending(r => r.CreationDate)
                .ThenByDescending(r => r.Id);

            var totalCount = await ordered.CountAsync();
            var rfqs = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var rfqIds = rfqs.Select(r => r.Id).ToList();
            var contracted = await _context.GetContractedQuantitiesAsync(rfqIds);
            var ordered2 = await _context.GetOrderedQuantitiesAsync(rfqIds);

            var items = rfqs.Select(r =>
            {
                var (statusAr, statusEn) = r.Status.GetBilingualNames();
                var con = contracted.GetValueOrDefault(r.Id, 0m);
                var ord = ordered2.GetValueOrDefault(r.Id, 0m);
                return new RFQListItemDto
                {
                    Id = r.Id,
                    Number = r.Number,
                    CreationDate = r.CreationDate,
                    AnnualPlanID = r.AnnualPlanID,
                    AnnualPlanNumber = r.AnnualPlan?.Number,
                    Status = r.Status,
                    StatusNameEn = statusEn,
                    StatusNameAr = statusAr,
                    MaterialId = r.MaterialId,
                    MaterialNumber = r.Material.MaterialNumber,
                    MaterialNameEn = r.Material.NameEn,
                    MaterialNameAr = r.Material.NameAr,
                    Quantity = r.Quantity,
                    ContractedQuantity = con,
                    OrderedQuantity = ord,
                    ContractedPercentage = RfqFulfillment.Percentage(con, r.Quantity),
                    OrderedPercentage = RfqFulfillment.Percentage(ord, r.Quantity),
                    IsActive = r.IsActive
                };
            }).ToList();

            return Ok(new PagedResultDto<RFQListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var exists = await _context.RFQs.AnyAsync(r => r.Id == id);
            if (!exists) return NotFound(new { message = "RFQ not found" });
            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var rfq = await _context.RFQs.FindAsync(id);
            if (rfq is null) return NotFound(new { message = "RFQ not found" });

            rfq.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User, dto.IsActive ? "ActivateRFQ" : "DeactivateRFQ",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} RFQ '{rfq.Number}' (Id: {rfq.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"RFQ {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rfq = await _context.RFQs.FindAsync(id);
            if (rfq is null) return NotFound(new { message = "RFQ not found" });

            _context.RFQs.Remove(rfq);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(rfq);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User, "DeleteRFQ",
                $"Deleted RFQ '{rfq.Number}' (Id: {rfq.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "RFQ deleted successfully" });
        }
        [HttpGet("{id}/contractreviews")]
        public async Task<IActionResult> GetContractReviews(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var exists = await _context.RFQs.AnyAsync(r => r.Id == id);
            if (!exists) return NotFound(new { message = "RFQ not found" });

            var query = _context.ContractReviews
                .Where(c => c.RFQId == id)
                .Include(c => c.Supplier)
                .Include(c => c.RFQ).ThenInclude(r => r.Material)
                .OrderByDescending(c => c.CreationDate).ThenByDescending(c => c.Id);

            var totalCount = await query.CountAsync();
            var reviews = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var fulfilled = await _context.GetFulfilledQuantitiesAsync(reviews.Select(r => r.Id));

            return Ok(new PagedResultDto<ContractReviewResponseDto>
            {
                Items = reviews.Select(r =>
                {
                    var (sAr, sEn) = r.Status.GetBilingualNames();
                    var (cmAr, cmEn) = r.ConnectionMethod.GetBilingualNames();
                    var (rfqStatusAr, rfqStatusEn) = r.RFQ.Status.GetBilingualNames();
                    var f = fulfilled.GetValueOrDefault(r.Id, 0m);
                    return new ContractReviewResponseDto
                    {
                        Id = r.Id,
                        Number = r.Number,
                        CreationDate = r.CreationDate,
                        ExpectedDate = r.ExpectedDate,
                        SupplierId = r.SupplierId,
                        SupplierNumber = r.Supplier.SupplierNumber,
                        SupplierNameEn = r.Supplier.NameEn,
                        SupplierNameAr = r.Supplier.NameAr,
                        ConnectionMethod = r.ConnectionMethod,
                        ConnectionMethodNameEn = cmEn,
                        ConnectionMethodNameAr = cmAr,
                        Status = r.Status,
                        StatusNameEn = sEn,
                        StatusNameAr = sAr,
                        MaterialId = r.RFQ.MaterialId,
                        MaterialNumber = r.RFQ.Material.MaterialNumber,
                        MaterialNameEn = r.RFQ.Material.NameEn,
                        MaterialNameAr = r.RFQ.Material.NameAr,
                        RFQId = r.RFQId,
                        RFQNumber = r.RFQ.Number,
                        RFQStatus = r.RFQ.Status,
                        RFQStatusNameEn = rfqStatusEn,
                        RFQStatusNameAr = rfqStatusAr,
                        Quantity = r.Quantity,
                        FulfilledQuantity = f,
                        RemainingQuantity = Math.Max(0, r.Quantity - f),
                        FulfillmentPercentage = ContractReviewFulfillment.Percentage(f, r.Quantity),
                        IsFulfilled = f >= r.Quantity,
                        Notes = r.Notes,
                        IsActive = r.IsActive
                    };
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}/purchaseorders")]
        public async Task<IActionResult> GetPurchaseOrders(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var exists = await _context.RFQs.AnyAsync(r => r.Id == id);
            if (!exists) return NotFound(new { message = "RFQ not found" });

            var query = _context.PurchaseOrders
                .Where(p => p.ContractReview.RFQId == id)
                .Include(p => p.CertificateOfAnalysis).ThenInclude(c => c.Sample)
                .Include(p => p.ContractReview).ThenInclude(r => r.Supplier)
                .Include(p => p.ContractReview).ThenInclude(r => r.RFQ).ThenInclude(rfq => rfq.Material)
                .OrderByDescending(p => p.CreationDate).ThenByDescending(p => p.Id);

            var totalCount = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var fulfilled = await _context.GetFulfilledQuantitiesAsync(orders.Select(o => o.ContractReviewId));

            return Ok(new PagedResultDto<PurchaseOrderResponseDto>
            {
                Items = orders.Select(o =>
                {
                    var review = o.ContractReview;
                    var m = review.RFQ.Material;
                    var cert = o.CertificateOfAnalysis;
                    var (statusAr, statusEn) = o.Status.GetBilingualNames();
                    var (resAr, resEn) = cert.Result.GetBilingualNames();
                    var f = fulfilled.GetValueOrDefault(o.ContractReviewId, 0m);
                    return new PurchaseOrderResponseDto
                    {
                        Id = o.Id,
                        Number = o.Number,
                        CreationDate = o.CreationDate,
                        DeliveryDate = o.DeliveryDate,
                        Quantity = o.Quantity,
                        Status = o.Status,
                        StatusNameEn = statusEn,
                        StatusNameAr = statusAr,
                        CertificateOfAnalysisId = cert.Id,
                        CertificateNumber = cert.Number,
                        CertificateAnalysisDate = cert.AnalysisDate,
                        CertificateResult = cert.Result,
                        CertificateResultNameEn = resEn,
                        CertificateResultNameAr = resAr,
                        ContractReviewId = review.Id,
                        ContractReviewNumber = review.Number,
                        ContractReviewQuantity = review.Quantity,
                        ContractReviewFulfilledQuantity = f,
                        ContractReviewRemainingQuantity = Math.Max(0, review.Quantity - f),
                        ContractReviewFulfillmentPercentage = ContractReviewFulfillment.Percentage(f, review.Quantity),
                        ContractReviewIsFulfilled = f >= review.Quantity,
                        SupplierId = review.SupplierId,
                        SupplierNumber = review.Supplier.SupplierNumber,
                        SupplierNameEn = review.Supplier.NameEn,
                        SupplierNameAr = review.Supplier.NameAr,
                        MaterialId = m.Id,
                        MaterialNumber = m.MaterialNumber,
                        MaterialNameEn = m.NameEn,
                        MaterialNameAr = m.NameAr,
                        Notes = o.Notes,
                        IsActive = o.IsActive
                    };
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        private async Task<RFQResponseDto> ToResponseDto(int id)
        {
            var rfq = await _context.RFQs
                .Include(r => r.AnnualPlan)
                .Include(r => r.Material)
                .FirstAsync(r => r.Id == id);

            var contracted = await _context.GetContractedQuantitiesAsync(new[] { id });
            var ordered = await _context.GetOrderedQuantitiesAsync(new[] { id });
            return ToDto(rfq, contracted.GetValueOrDefault(id, 0m), ordered.GetValueOrDefault(id, 0m));
        }

        private static RFQResponseDto ToDto(RFQ r, decimal contracted, decimal ordered)
        {
            var (statusAr, statusEn) = r.Status.GetBilingualNames();

            return new RFQResponseDto
            {
                Id = r.Id,
                Number = r.Number,
                CreationDate = r.CreationDate,
                AnnualPlanID = r.AnnualPlanID,
                AnnualPlanNumber = r.AnnualPlan?.Number,
                AnnualPlanDate = r.AnnualPlan?.AnnualPlanDate,
                Status = r.Status,
                StatusNameEn = statusEn,
                StatusNameAr = statusAr,
                MaterialId = r.MaterialId,
                MaterialNumber = r.Material.MaterialNumber,
                MaterialNameEn = r.Material.NameEn,
                MaterialNameAr = r.Material.NameAr,
                Quantity = r.Quantity,
                ContractedQuantity = contracted,
                OrderedQuantity = ordered,
                ContractedPercentage = RfqFulfillment.Percentage(contracted, r.Quantity),
                OrderedPercentage = RfqFulfillment.Percentage(ordered, r.Quantity),
                Notes = r.Notes,
                IsActive = r.IsActive
            };
        }
    }
}