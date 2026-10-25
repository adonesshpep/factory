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
    [Route("api/scales")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ScalesController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly AppDbContext _context;

        public ScalesController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateScaleDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department is null || !department.IsActive)
            {
                return BadRequest(new { message = "Department does not exist or is inactive" });
            }

            var scale = new Scale
            {
                ScaleNumber = dto.ScaleNumber.Trim(),
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                DepartmentId = department.Id
            };

            try
            {
                _context.Scales.Add(scale);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A scale with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateScale",
                $"Created scale '{scale.NameEn}' / '{scale.NameAr}' (Id: {scale.Id}, Number: {scale.ScaleNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(scale));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateScaleDto dto)
        {
            var scale = await _context.Scales.FindAsync(id);
            if (scale is null)
            {
                return NotFound(new { message = "Scale not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.ScaleNumber))
                scale.ScaleNumber = dto.ScaleNumber.Trim();

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) scale.NameEn = nameEn;
            if (nameAr != null) scale.NameAr = nameAr;

            if (dto.Notes != null)
                scale.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(dto.DepartmentId.Value);
                if (department is null || !department.IsActive)
                    return BadRequest(new { message = "Department does not exist or is inactive" });
                scale.DepartmentId = department.Id;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A scale with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateScale",
                $"Updated scale (Id: {scale.Id}, Number: {scale.ScaleNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(scale));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Scales
                .Include(m => m.Department)
                .OrderBy(m => m.Id);

            var totalCount = await query.CountAsync();

            var scales = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ScaleResponseDto
                {
                    Id = m.Id,
                    ScaleNumber = m.ScaleNumber,
                    NameEn = m.NameEn,
                    NameAr = m.NameAr,
                    Notes = m.Notes,
                    DepartmentId = m.DepartmentId,
                    DepartmentNameEn = m.Department.NameEn,
                    DepartmentNameAr = m.Department.NameAr,
                    IsActive = m.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<ScaleResponseDto>
            {
                Items = scales,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var scale = await _context.Scales.FindAsync(id);
            if (scale is null)
            {
                return NotFound(new { message = "Scale not found" });
            }

            scale.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateScale" : "DeactivateScale",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} scale '{scale.NameEn}' / '{scale.NameAr}' (Id: {scale.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Scale {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var scale = await _context.Scales.FindAsync(id);
            if (scale is null)
            {
                return NotFound(new { message = "Scale not found" });
            }

            _context.Scales.Remove(scale);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(scale);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteScale",
                $"Deleted scale '{scale.NameEn}' / '{scale.NameAr}' (Id: {scale.Id}, Number: {scale.ScaleNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Scale deleted successfully" });
        }

        private async Task<ScaleResponseDto> ToResponseDto(Scale scale)
        {
            await _context.Entry(scale).Reference(m => m.Department).LoadAsync();

            return new ScaleResponseDto
            {
                Id = scale.Id,
                ScaleNumber = scale.ScaleNumber,
                NameEn = scale.NameEn,
                NameAr = scale.NameAr,
                Notes = scale.Notes,
                DepartmentId = scale.DepartmentId,
                DepartmentNameEn = scale.Department.NameEn,
                DepartmentNameAr = scale.Department.NameAr,
                IsActive = scale.IsActive
            };
        }

        private static string FieldFromIndex(string sqlMessage) =>
            sqlMessage.Contains("ix_scales_scale_number") ? "scale number" :
            sqlMessage.Contains("ix_scales_name_en") ? "English name" :
            sqlMessage.Contains("ix_scales_name_ar") ? "Arabic name" :
            "value";
    }
}
