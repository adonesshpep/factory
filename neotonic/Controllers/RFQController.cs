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

        public RFQController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
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

            try
            {
                _context.RFQs.Add(rfq);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "An RFQ with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateRFQ",
                $"Created RFQ '{rfq.Number}' (Id: {rfq.Id}, Material: {material.MaterialNumber}, Quantity: {rfq.Quantity})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(rfq.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateRFQDto dto)
        {
            var rfq = await _context.RFQs.FindAsync(id);
            if (rfq is null)
                return NotFound(new { message = "RFQ not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number))
                rfq.Number = dto.Number.Trim();

            if (dto.AnnualPlanID.HasValue)
            {
                var plan = await _context.AnnualPlans.FindAsync(dto.AnnualPlanID.Value);
                if (plan is null || !plan.IsActive)
                    return BadRequest(new { message = "Annual plan does not exist or is inactive" });
                rfq.AnnualPlanID = plan.Id;
            }

            if (dto.Status.HasValue)
                rfq.Status = dto.Status.Value;

            if (dto.MaterialId.HasValue)
            {
                var material = await _context.Materials.FindAsync(dto.MaterialId.Value);
                if (material is null || !material.IsActive)
                    return BadRequest(new { message = "Material does not exist or is inactive" });
                rfq.MaterialId = material.Id;
            }

            if (dto.Quantity.HasValue)
                rfq.Quantity = dto.Quantity.Value;

            if (dto.Notes != null)
                rfq.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "An RFQ with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateRFQ",
                $"Updated RFQ (Id: {rfq.Id}, Number: {rfq.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(rfq.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.RFQs
                .Include(r => r.AnnualPlan)
                .Include(r => r.Material)
                .OrderByDescending(r => r.CreationDate)
                .ThenByDescending(r => r.Id);

            var totalCount = await query.CountAsync();

            var rows = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Number,
                    r.CreationDate,
                    r.AnnualPlanID,
                    AnnualPlanNumber = r.AnnualPlan != null ? r.AnnualPlan.Number : null,
                    r.Status,
                    r.MaterialId,
                    r.Material.MaterialNumber,
                    r.Material.NameEn,
                    r.Material.NameAr,
                    r.Quantity,
                    r.IsActive
                })
                .ToListAsync();

            // Enum labels come from [Description], which SQL can't see.
            var items = rows.Select(r =>
            {
                var (statusAr, statusEn) = r.Status.GetBilingualNames();
                return new RFQListItemDto
                {
                    Id = r.Id,
                    Number = r.Number,
                    CreationDate = r.CreationDate,
                    AnnualPlanID = r.AnnualPlanID,
                    AnnualPlanNumber = r.AnnualPlanNumber,
                    Status = r.Status,
                    StatusNameEn = statusEn,
                    StatusNameAr = statusAr,
                    MaterialId = r.MaterialId,
                    MaterialNumber = r.MaterialNumber,
                    MaterialNameEn = r.NameEn,
                    MaterialNameAr = r.NameAr,
                    Quantity = r.Quantity,
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
            if (!exists)
                return NotFound(new { message = "RFQ not found" });

            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var rfq = await _context.RFQs.FindAsync(id);
            if (rfq is null)
                return NotFound(new { message = "RFQ not found" });

            rfq.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateRFQ" : "DeactivateRFQ",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} RFQ '{rfq.Number}' (Id: {rfq.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"RFQ {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rfq = await _context.RFQs.FindAsync(id);
            if (rfq is null)
                return NotFound(new { message = "RFQ not found" });

            _context.RFQs.Remove(rfq);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(rfq);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteRFQ",
                $"Deleted RFQ '{rfq.Number}' (Id: {rfq.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "RFQ deleted successfully" });
        }

        private async Task<RFQResponseDto> ToResponseDto(int id)
        {
            var rfq = await _context.RFQs
                .Include(r => r.AnnualPlan)
                .Include(r => r.Material)
                .FirstAsync(r => r.Id == id);

            return ToDto(rfq);
        }

        private static RFQResponseDto ToDto(RFQ r)
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
                Notes = r.Notes,
                IsActive = r.IsActive
            };
        }
    }
}