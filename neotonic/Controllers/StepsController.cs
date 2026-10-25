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
    [Route("api/steps")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class StepsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public StepsController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateStepDto dto)
        {
            var nameEn = dto.StepNameEn?.Trim();
            var nameAr = dto.StepNameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var stepType = await _context.StepTypes.FindAsync(dto.StepTypeId);
            if (stepType is null || !stepType.IsActive)
            {
                return BadRequest(new { message = "Step type does not exist or is inactive" });
            }
            var color =dto.Color?.Trim();
            if (color is  null) {
                color = "#FFFFFF";
            }

            var step = new Step
            {
                StepNumber = dto.StepNumber!.Value,
                StepNameEn = resolvedEn,
                StepNameAr = resolvedAr,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                StepTypeId = stepType.Id,
                Color = color,
            };
            if (dto.BoxPathId.HasValue)
            {
                var boxPath = await _context.BoxPaths.FindAsync(dto.BoxPathId);
                if (boxPath is null || !boxPath.IsActive)
                {
                    return BadRequest(new { message = "Box path does not exist or is inactive" });
                }
                step.BoxPathId = boxPath.Id;

            }

            try
            {
                _context.Steps.Add(step);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                var field =
                    sqlEx.Message.Contains("ix_steps_step_number") ? "step number" :
                    sqlEx.Message.Contains("ix_steps_step_name_en") ? "English name" :
                    sqlEx.Message.Contains("ix_steps_step_name_ar") ? "Arabic name" :
                    "value";

                return Conflict(new { message = $"A step with this {field} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateStep",
                $"Created step '{step.StepNameEn}' / '{step.StepNameAr}' (Id: {step.Id}, Number: {step.StepNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(step));
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateStepDto dto)
        {
            var step = await _context.Steps.FindAsync(id);
            if (step is null)
            {
                return NotFound(new { message = "Step not found" });
            }

            var nameEn = string.IsNullOrWhiteSpace(dto.StepNameEn) ? null : dto.StepNameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.StepNameAr) ? null : dto.StepNameAr.Trim();

            if (nameEn != null) step.StepNameEn = nameEn;
            if (nameAr != null) step.StepNameAr = nameAr;
            if (dto.Color != null) step.Color = string.IsNullOrWhiteSpace(dto.Color) ? "#FFFFFF" : dto.Color.Trim();

            if (dto.StepNumber.HasValue)
                step.StepNumber = dto.StepNumber.Value;

            if (dto.Notes != null)
                step.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.BoxPathId.HasValue)
            {
                var boxPath = await _context.BoxPaths.FindAsync(dto.BoxPathId);
                if (boxPath is null || !boxPath.IsActive)
                {
                    return BadRequest(new { message = "Box path does not exist or is inactive" });
                }
                step.BoxPathId = boxPath.Id;
            }

            if (dto.StepTypeId.HasValue)
            {
                var stepType = await _context.StepTypes.FindAsync(dto.StepTypeId.Value);
                if (stepType is null || !stepType.IsActive)
                    return BadRequest(new { message = "Step type does not exist or is inactive" });
                step.StepTypeId = stepType.Id;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                var field =
                    sqlEx.Message.Contains("ix_steps_step_number") ? "step number" :
                    sqlEx.Message.Contains("ix_steps_step_name_en") ? "English name" :
                    sqlEx.Message.Contains("ix_steps_step_name_ar") ? "Arabic name" :
                    "value";

                return Conflict(new { message = $"A step with this {field} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateStep",
                $"Updated step (Id: {step.Id}, Number: {step.StepNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(step));
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Steps
                .Include(s => s.StepType)
                .Include(s => s.BoxPath)
                .OrderBy(s => s.StepNumber);

            var totalCount = await query.CountAsync();

            var steps = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StepResponseDto
                {
                    Id = s.Id,
                    StepNumber = s.StepNumber,
                    StepNameEn = s.StepNameEn,
                    StepNameAr = s.StepNameAr,
                    Notes = s.Notes,
                    BoxPathId = s.BoxPathId,
                    BoxPathName = s.BoxPath != null ? s.BoxPath.Name : null,
                    BoxPathCode = s.BoxPath != null ? s.BoxPath.Code : null,
                    StepTypeId = s.StepTypeId,
                    StepTypeNameEn = s.StepType.NameEn,
                    StepTypeNameAr = s.StepType.NameAr,
                    Color=s.Color,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<StepResponseDto>
            {
                Items = steps,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var step = await _context.Steps.FindAsync(id);
            if (step is null)
            {
                return NotFound(new { message = "Step not found" });
            }

            step.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateStep" : "DeactivateStep",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} step '{step.StepNameEn}' / '{step.StepNameAr}' (Id: {step.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Step {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var step = await _context.Steps.FindAsync(id);
            if (step is null)
            {
                return NotFound(new { message = "Step not found" });
            }

            _context.Steps.Remove(step);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(step);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteStep",
                $"Deleted step '{step.StepNameEn}' / '{step.StepNameAr}' (Id: {step.Id}, Number: {step.StepNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Step deleted successfully" });
        }
        private async Task<StepResponseDto> ToResponseDto(Step step)
        {
            await _context.Entry(step).Reference(s => s.StepType).LoadAsync();
            await _context.Entry(step).Reference(s => s.BoxPath).LoadAsync();

            return new StepResponseDto
            {
                Id = step.Id,
                StepNumber = step.StepNumber,
                StepNameEn = step.StepNameEn,
                StepNameAr = step.StepNameAr,
                Notes = step.Notes,
                BoxPathId = step.BoxPathId,
                BoxPathName = step.BoxPath != null ? step.BoxPath.Name : null,
                BoxPathCode = step.BoxPath != null ? step.BoxPath.Code : null,
                StepTypeId = step.StepTypeId,
                StepTypeNameEn = step.StepType.NameEn,
                StepTypeNameAr = step.StepType.NameAr,
                Color=step.Color,
                IsActive = step.IsActive
            };
        }
    }
}
