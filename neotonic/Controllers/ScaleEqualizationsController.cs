using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/scaleequalizations")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ScaleEqualizationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public ScaleEqualizationsController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateScaleEqualizationDto dto)
        {
            var scale = await _context.Scales.FindAsync(dto.ScaleId);
            if (scale is null || !scale.IsActive)
            {
                return BadRequest(new { message = "Scale does not exist or is inactive" });
            }

            var worker = await _context.Workers.FindAsync(dto.WorkerId);
            if (worker is null || !worker.IsActive)
            {
                return BadRequest(new { message = "Worker does not exist or is inactive" });
            }

            var equalization = new ScaleEqualization
            {
                Number = dto.Number.Trim(),
                EqualizationDate = dto.EqualizationDate!.Value,
                ScaleId = scale.Id,
                WorkerId = worker.Id,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try
            {
                _context.ScaleEqualizations.Add(equalization);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A scale equalization with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateScaleEqualization",
                $"Created scale equalization '{equalization.Number}' (Id: {equalization.Id}, Scale: {scale.ScaleNumber}, Worker: {worker.WorkerNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(equalization));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateScaleEqualizationDto dto)
        {
            var equalization = await _context.ScaleEqualizations.FindAsync(id);
            if (equalization is null)
            {
                return NotFound(new { message = "Scale equalization not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Number))
                equalization.Number = dto.Number.Trim();

            if (dto.EqualizationDate.HasValue)
                equalization.EqualizationDate = dto.EqualizationDate.Value;

            if (dto.Notes != null)
                equalization.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.ScaleId.HasValue)
            {
                var scale = await _context.Scales.FindAsync(dto.ScaleId.Value);
                if (scale is null || !scale.IsActive)
                    return BadRequest(new { message = "Scale does not exist or is inactive" });
                equalization.ScaleId = scale.Id;
            }

            if (dto.WorkerId.HasValue)
            {
                var worker = await _context.Workers.FindAsync(dto.WorkerId.Value);
                if (worker is null || !worker.IsActive)
                    return BadRequest(new { message = "Worker does not exist or is inactive" });
                equalization.WorkerId = worker.Id;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A scale equalization with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateScaleEqualization",
                $"Updated scale equalization (Id: {equalization.Id}, Number: {equalization.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(equalization));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? scaleId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ScaleEqualizations.AsQueryable();

            if (scaleId.HasValue)
                query = query.Where(e => e.ScaleId == scaleId.Value);

            var ordered = query
                .OrderByDescending(e => e.EqualizationDate)
                .ThenByDescending(e => e.Id);

            var totalCount = await ordered.CountAsync();

            var items = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ScaleEqualizationResponseDto
                {
                    Id = e.Id,
                    Number = e.Number,
                    EqualizationDate = e.EqualizationDate,
                    ScaleId = e.ScaleId,
                    ScaleNumber = e.Scale.ScaleNumber,
                    ScaleNameEn = e.Scale.NameEn,
                    ScaleNameAr = e.Scale.NameAr,
                    WorkerId = e.WorkerId,
                    WorkerNumber = e.Worker.WorkerNumber,
                    WorkerNameEn = e.Worker.NameEn,
                    WorkerNameAr = e.Worker.NameAr,
                    Notes = e.Notes
                })
                .ToListAsync();

            return Ok(new PagedResultDto<ScaleEqualizationResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var equalization = await _context.ScaleEqualizations.FindAsync(id);
            if (equalization is null)
            {
                return NotFound(new { message = "Scale equalization not found" });
            }

            _context.ScaleEqualizations.Remove(equalization);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(equalization);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteScaleEqualization",
                $"Deleted scale equalization '{equalization.Number}' (Id: {equalization.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Scale equalization deleted successfully" });
        }

        private async Task<ScaleEqualizationResponseDto> ToResponseDto(ScaleEqualization equalization)
        {
            await _context.Entry(equalization).Reference(e => e.Scale).LoadAsync();
            await _context.Entry(equalization).Reference(e => e.Worker).LoadAsync();

            return new ScaleEqualizationResponseDto
            {
                Id = equalization.Id,
                Number = equalization.Number,
                EqualizationDate = equalization.EqualizationDate,
                ScaleId = equalization.ScaleId,
                ScaleNumber = equalization.Scale.ScaleNumber,
                ScaleNameEn = equalization.Scale.NameEn,
                ScaleNameAr = equalization.Scale.NameAr,
                WorkerId = equalization.WorkerId,
                WorkerNumber = equalization.Worker.WorkerNumber,
                WorkerNameEn = equalization.Worker.NameEn,
                WorkerNameAr = equalization.Worker.NameAr,
                Notes = equalization.Notes
            };
        }
    }
}