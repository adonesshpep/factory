using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Route("api/steptypes")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class StepTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public StepTypesController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateStepTypeDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var stepType = new StepType
            {
                NameEn = resolvedEn,
                NameAr = resolvedAr
            };

            try
            {
                _context.StepTypes.Add(stepType);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A step type with this name already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateStepType",
                $"Created step type '{stepType.NameEn}' / '{stepType.NameAr}' (Id: {stepType.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new StepTypeResponseDto
            {
                Id = stepType.Id,
                NameEn = stepType.NameEn,
                NameAr = stepType.NameAr,
                IsActive = stepType.IsActive
            });
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateStepTypeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NameEn)) dto.NameEn = null;
            if (string.IsNullOrWhiteSpace(dto.NameAr)) dto.NameAr = null;
            if (dto.NameEn == null && dto.NameAr == null)
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var stepType = await _context.StepTypes.FindAsync(id);
            if (stepType == null)
            {
                return NotFound(new { message = "Step type not found" });
            }

            if (dto.NameEn != null) stepType.NameEn = dto.NameEn.Trim();
            if (dto.NameAr != null) stepType.NameAr = dto.NameAr.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A step type with this name already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateStepType",
                $"Updated step type (Id: {stepType.Id}) to '{stepType.NameEn}' / '{stepType.NameAr}'",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new StepTypeResponseDto
            {
                Id = stepType.Id,
                NameEn = stepType.NameEn,
                NameAr = stepType.NameAr,
                IsActive = stepType.IsActive
            });
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.StepTypes.OrderBy(s => s.Id);
            var totalCount = await query.CountAsync();

            var stepTypes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StepTypeResponseDto
                {
                    Id = s.Id,
                    NameEn = s.NameEn,
                    NameAr = s.NameAr,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<StepTypeResponseDto>
            {
                Items = stepTypes,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var stepType = await _context.StepTypes.FindAsync(id);
            if (stepType == null)
            {
                return NotFound(new { message = "Step type not found" });
            }

            stepType.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateStepType" : "DeactivateStepType",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} step type '{stepType.NameEn}' / '{stepType.NameAr}' (Id: {stepType.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Step type {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stepType = await _context.StepTypes.FindAsync(id);
            if (stepType == null)
            {
                return NotFound(new { message = "Step type not found" });
            }

            _context.StepTypes.Remove(stepType);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(stepType);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteStepType",
                $"Deleted step type '{stepType.NameEn}' / '{stepType.NameAr}' (Id: {stepType.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Step type deleted successfully" });
        }

    }
}
