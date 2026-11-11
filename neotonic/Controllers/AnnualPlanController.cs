using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Responses;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/annualplans")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    [Authorize(Roles ="Admin")]
    public class AnnualPlansController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public AnnualPlansController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateAnnualPlanDto dto)
        {
            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department is null || !department.IsActive)
                return BadRequest(new { message = "Department does not exist or is inactive" });

            var materialType = await _context.MaterialTypes.FindAsync(dto.MaterialTypeId);
            if (materialType is null || !materialType.IsActive)
                return BadRequest(new { message = "Material type does not exist or is inactive" });

            var materialIds = dto.Details.Select(d => d.MaterialId).ToList();
            if (materialIds.Distinct().Count() != materialIds.Count)
                return BadRequest(new { message = "The same material cannot appear twice in one plan" });

            var details = new List<AnnualPlanDetail>();
            foreach (var d in dto.Details)
            {
                var material = await _context.Materials.FindAsync(d.MaterialId);
                if (material is null || !material.IsActive)
                    return BadRequest(new { message = $"Material {d.MaterialId} does not exist or is inactive" });

                details.Add(new AnnualPlanDetail
                {
                    MaterialId = material.Id,
                    Quantity = d.Quantity,
                    Notes = string.IsNullOrWhiteSpace(d.Notes) ? null : d.Notes.Trim()
                });
            }

            var plan = new AnnualPlan
            {
                Number = dto.Number.Trim(),
                DepartmentId = department.Id,
                MaterialTypeId = materialType.Id,
                AnnualPlanDate = dto.AnnualPlanDate ?? DateTime.UtcNow,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                Details = details
            };

            try
            {
                _context.AnnualPlans.Add(plan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "An annual plan with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User, "CreateAnnualPlan",
                $"Created annual plan '{plan.Number}' (Id: {plan.Id}) with {details.Count} material(s)",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(plan.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateAnnualPlanDto dto)
        {
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null) return NotFound(new { message = "Annual plan not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number)) plan.Number = dto.Number.Trim();

            if (dto.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(dto.DepartmentId.Value);
                if (department is null || !department.IsActive)
                    return BadRequest(new { message = "Department does not exist or is inactive" });
                plan.DepartmentId = department.Id;
            }

            if (dto.MaterialTypeId.HasValue)
            {
                var materialType = await _context.MaterialTypes.FindAsync(dto.MaterialTypeId.Value);
                if (materialType is null || !materialType.IsActive)
                    return BadRequest(new { message = "Material type does not exist or is inactive" });
                plan.MaterialTypeId = materialType.Id;
            }

            if (dto.AnnualPlanDate.HasValue) plan.AnnualPlanDate = dto.AnnualPlanDate.Value;

            if (dto.Notes != null)
                plan.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.Details is not null)
            {
                var materialIds = dto.Details.Select(d => d.MaterialId).ToList();
                if (materialIds.Distinct().Count() != materialIds.Count)
                    return BadRequest(new { message = "The same material cannot appear twice in one plan" });

                var existing = await _context.AnnualPlanDetails
                    .Where(x => x.AnnualPlanId == id)
                    .ToDictionaryAsync(x => x.MaterialId);

                var newMaterialIds = materialIds.Where(m => !existing.ContainsKey(m)).ToList();
                if (newMaterialIds.Count > 0)
                {
                    var validIds = await _context.Materials
                        .Where(m => newMaterialIds.Contains(m.Id) && m.IsActive)
                        .Select(m => m.Id).ToListAsync();
                    var invalidIds = newMaterialIds.Except(validIds).ToList();
                    if (invalidIds.Count > 0)
                        return BadRequest(new { message = $"Materials do not exist or are inactive: {string.Join(", ", invalidIds)}" });
                }

                foreach (var d in dto.Details)
                {
                    var notes = string.IsNullOrWhiteSpace(d.Notes) ? null : d.Notes.Trim();
                    if (existing.TryGetValue(d.MaterialId, out var detail))
                    {
                        detail.Quantity = d.Quantity;
                        detail.Notes = notes;
                    }
                    else
                    {
                        _context.AnnualPlanDetails.Add(new AnnualPlanDetail
                        {
                            AnnualPlanId = plan.Id,
                            MaterialId = d.MaterialId,
                            Quantity = d.Quantity,
                            Notes = notes
                        });
                    }
                }

                var toRemove = existing.Values.Where(x => !materialIds.Contains(x.MaterialId)).ToList();
                _context.AnnualPlanDetails.RemoveRange(toRemove);
            }

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "An annual plan with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User, "UpdateAnnualPlan",
                $"Updated annual plan (Id: {plan.Id}, Number: {plan.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(plan.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.AnnualPlans
                .Include(p => p.Department)
                .Include(p => p.MaterialType)
                .Include(p => p.Details)
                .OrderBy(p => p.Id);

            var totalCount = await query.CountAsync();
            var plans = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var planIds = plans.Select(p => p.Id).ToList();

            var requested = await _context.GetRequestedByMaterialAsync(planIds);
            var contracted = await _context.GetContractedByMaterialAsync(planIds);
            var ordered = await _context.GetOrderedByMaterialAsync(planIds);

            var items = plans.Select(p =>
            {
                var lines = p.Details.Select(d => new
                {
                    Planned = d.Quantity,
                    Req = requested.GetValueOrDefault((p.Id, d.MaterialId), 0m),
                    Con = contracted.GetValueOrDefault((p.Id, d.MaterialId), 0m),
                    Ord = ordered.GetValueOrDefault((p.Id, d.MaterialId), 0m)
                }).ToList();

                var (statusAr, statusEn) = p.AnnualStatuses.GetBilingualNames();
                return new AnnualPlanListItemDto
                {
                    Id = p.Id,
                    Number = p.Number,
                    DepartmentNameEn = p.Department.NameEn,
                    DepartmentNameAr = p.Department.NameAr,
                    MaterialTypeNameEn = p.MaterialType.NameEn,
                    MaterialTypeNameAr = p.MaterialType.NameAr,
                    AnnualPlanDate = p.AnnualPlanDate,
                    AnnualStatuses = p.AnnualStatuses,
                    AnnualPlanStatusNameEn = statusEn,
                    AnnualPlanStatusNameAr = statusAr,
                    IsActive = p.IsActive,
                    DetailCount = p.Details.Count,
                    RequestedPercentage = AnnualPlanFulfillment.PlanPercentage(lines.Select(l => (l.Planned, l.Req))),
                    ContractedPercentage = AnnualPlanFulfillment.PlanPercentage(lines.Select(l => (l.Planned, l.Con))),
                    OrderedPercentage = AnnualPlanFulfillment.PlanPercentage(lines.Select(l => (l.Planned, l.Ord)))
                };
            }).ToList();

            return Ok(new PagedResultDto<AnnualPlanListItemDto>
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
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null) return NotFound(new { message = "Annual plan not found" });
            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null) return NotFound(new { message = "Annual plan not found" });

            plan.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User, dto.IsActive ? "ActivateAnnualPlan" : "DeactivateAnnualPlan",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} annual plan '{plan.Number}' (Id: {plan.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Annual plan {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null) return NotFound(new { message = "Annual plan not found" });

            _context.AnnualPlans.Remove(plan);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(plan);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User, "DeleteAnnualPlan",
                $"Deleted annual plan '{plan.Number}' (Id: {plan.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Annual plan deleted successfully" });
        }
        [HttpGet("{id}/rfqs")]
        public async Task<IActionResult> GetRfqs(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var exists = await _context.AnnualPlans.AnyAsync(p => p.Id == id);
            if (!exists) return NotFound(new { message = "Annual plan not found" });

            var query = _context.RFQs
                .Where(r => r.AnnualPlanID == id)
                .Include(r => r.Material)
                .OrderByDescending(r => r.CreationDate).ThenByDescending(r => r.Id);

            var totalCount = await query.CountAsync();
            var rfqs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new PagedResultDto<RFQListItemDto>
            {
                Items = rfqs.Select(r =>
                {
                    var (statusAr, statusEn) = r.Status.GetBilingualNames();
                    return new RFQListItemDto
                    {
                        Id = r.Id,
                        Number = r.Number,
                        CreationDate = r.CreationDate,
                        AnnualPlanID = r.AnnualPlanID,
                        AnnualPlanNumber = null,
                        Status = r.Status,
                        StatusNameEn = statusEn,
                        StatusNameAr = statusAr,
                        MaterialId = r.MaterialId,
                        MaterialNumber = r.Material.MaterialNumber,
                        MaterialNameEn = r.Material.NameEn,
                        MaterialNameAr = r.Material.NameAr,
                        Quantity = r.Quantity,
                        IsActive = r.IsActive
                    };
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}/contractreviews")]
        public async Task<IActionResult> GetContractReviews(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var exists = await _context.AnnualPlans.AnyAsync(p => p.Id == id);
            if (!exists) return NotFound(new { message = "Annual plan not found" });

            var query = _context.ContractReviews
                .Where(c => c.RFQ.AnnualPlanID == id)
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

            var exists = await _context.AnnualPlans.AnyAsync(p => p.Id == id);
            if (!exists) return NotFound(new { message = "Annual plan not found" });

            var query = _context.PurchaseOrders
                .Where(p => p.ContractReview.RFQ.AnnualPlanID == id)
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
        private async Task<AnnualPlanResponseDto> ToResponseDto(int planId)
        {
            var plan = await _context.AnnualPlans
                .Include(p => p.Department)
                .Include(p => p.MaterialType)
                .FirstAsync(p => p.Id == planId);

            var details = await _context.AnnualPlanDetails
                .Where(d => d.AnnualPlanId == planId)
                .Include(d => d.Material)
                .ToListAsync();

            var requested = await _context.GetRequestedByMaterialAsync(new[] { planId });
            var contracted = await _context.GetContractedByMaterialAsync(new[] { planId });
            var ordered = await _context.GetOrderedByMaterialAsync(new[] { planId });

            var detailDtos = details.Select(d =>
            {
                var planned = d.Quantity;
                var req = requested.GetValueOrDefault((planId, d.MaterialId), 0m);
                var con = contracted.GetValueOrDefault((planId, d.MaterialId), 0m);
                var ord = ordered.GetValueOrDefault((planId, d.MaterialId), 0m);

                return new AnnualPlanDetailResponseDto
                {
                    Id = d.Id,
                    MaterialId = d.MaterialId,
                    MaterialNumber = d.Material.MaterialNumber,
                    MaterialNameEn = d.Material.NameEn,
                    MaterialNameAr = d.Material.NameAr,
                    Quantity = planned,
                    Notes = d.Notes,
                    RequestedQuantity = req,
                    ContractedQuantity = con,
                    OrderedQuantity = ord,
                    RequestedPercentage = AnnualPlanFulfillment.Percentage(req, planned),
                    ContractedPercentage = AnnualPlanFulfillment.Percentage(con, planned),
                    OrderedPercentage = AnnualPlanFulfillment.Percentage(ord, planned)
                };
            }).ToList();

            var (statusAr, statusEn) = plan.AnnualStatuses.GetBilingualNames();

            return new AnnualPlanResponseDto
            {
                Id = plan.Id,
                Number = plan.Number,
                DepartmentId = plan.DepartmentId,
                DepartmentNameEn = plan.Department.NameEn,
                DepartmentNameAr = plan.Department.NameAr,
                MaterialTypeId = plan.MaterialTypeId,
                MaterialTypeNameEn = plan.MaterialType.NameEn,
                MaterialTypeNameAr = plan.MaterialType.NameAr,
                AnnualPlanDate = plan.AnnualPlanDate,
                AnnualStatuses = plan.AnnualStatuses,
                AnnualPlanStatusNameEn = statusEn,
                AnnualPlanStatusNameAr = statusAr,
                Notes = plan.Notes,
                IsActive = plan.IsActive,
                RequestedPercentage = AnnualPlanFulfillment.PlanPercentage(
                    detailDtos.Select(d => (d.Quantity, d.RequestedQuantity))),
                ContractedPercentage = AnnualPlanFulfillment.PlanPercentage(
                    detailDtos.Select(d => (d.Quantity, d.ContractedQuantity))),
                OrderedPercentage = AnnualPlanFulfillment.PlanPercentage(
                    detailDtos.Select(d => (d.Quantity, d.OrderedQuantity))),
                Details = detailDtos
            };
        }
    }
}