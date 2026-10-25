using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/annualplans")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
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
                User,
                "CreateAnnualPlan",
                $"Created annual plan '{plan.Number}' (Id: {plan.Id}) with {details.Count} material(s)",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(plan.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateAnnualPlanDto dto)
        {
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null)
                return NotFound(new { message = "Annual plan not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number))
                plan.Number = dto.Number.Trim();

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

            if (dto.AnnualPlanDate.HasValue)
                plan.AnnualPlanDate = dto.AnnualPlanDate.Value;

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

                // Only materials that are NOT already in the plan must be active
                var newMaterialIds = materialIds.Where(m => !existing.ContainsKey(m)).ToList();
                if (newMaterialIds.Count > 0)
                {
                    var validIds = await _context.Materials
                        .Where(m => newMaterialIds.Contains(m.Id) && m.IsActive)
                        .Select(m => m.Id)
                        .ToListAsync();

                    var invalidIds = newMaterialIds.Except(validIds).ToList();
                    if (invalidIds.Count > 0)
                        return BadRequest(new
                        {
                            message = $"Materials do not exist or are inactive: {string.Join(", ", invalidIds)}"
                        });
                }

                // 1. Edit existing rows in place, add new ones
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

                // 2. Delete the rows that were not sent in the request
                var toRemove = existing.Values
                    .Where(x => !materialIds.Contains(x.MaterialId))
                    .ToList();

                _context.AnnualPlanDetails.RemoveRange(toRemove);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "An annual plan with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateAnnualPlan",
                $"Updated annual plan (Id: {plan.Id}, Number: {plan.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(plan.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.AnnualPlans.OrderBy(p => p.Id);

            var totalCount = await query.CountAsync();

            var plans = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AnnualPlanListItemDto
                {
                    Id = p.Id,
                    Number = p.Number,
                    DepartmentNameEn = p.Department.NameEn,
                    DepartmentNameAr = p.Department.NameAr,
                    MaterialTypeNameEn = p.MaterialType.NameEn,
                    MaterialTypeNameAr = p.MaterialType.NameAr,
                    AnnualPlanDate = p.AnnualPlanDate,
                    AnnualStatuses = p.AnnualStatuses,
                    IsActive = p.IsActive,
                    DetailCount = p.Details.Count
                })
                .ToListAsync();

            // Enum labels come from [Description], which SQL can't see, so fill them in memory.
            foreach (var plan in plans)
            {
                var (statusAr, statusEn) = plan.AnnualStatuses.GetBilingualNames();
                plan.AnnualPlanStatusNameAr = statusAr;
                plan.AnnualPlanStatusNameEn = statusEn;
            }

            return Ok(new PagedResultDto<AnnualPlanListItemDto>
            {
                Items = plans,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null)
                return NotFound(new { message = "Annual plan not found" });

            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null)
                return NotFound(new { message = "Annual plan not found" });

            plan.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateAnnualPlan" : "DeactivateAnnualPlan",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} annual plan '{plan.Number}' (Id: {plan.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Annual plan {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.AnnualPlans.FindAsync(id);
            if (plan is null)
                return NotFound(new { message = "Annual plan not found" });

            _context.AnnualPlans.Remove(plan);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload =await _context.GetDeleteConflictsAsync(plan);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteAnnualPlan",
                $"Deleted annual plan '{plan.Number}' (Id: {plan.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Annual plan deleted successfully" });
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
                .Select(d => new AnnualPlanDetailResponseDto
                {
                    Id = d.Id,
                    MaterialId = d.MaterialId,
                    MaterialNumber = d.Material.MaterialNumber,
                    MaterialNameEn = d.Material.NameEn,
                    MaterialNameAr = d.Material.NameAr,
                    Quantity = d.Quantity,
                    Notes = d.Notes
                })
                .ToListAsync();
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
                Details = details
            };
        }
    }
}