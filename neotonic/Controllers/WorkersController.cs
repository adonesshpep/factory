using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/workers")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class WorkersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public WorkersController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateWorkerDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var birthDate = dto.BirthDate!.Value;
            if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return BadRequest(new { message = "Birth date cannot be in the future" });
            }

            var worker = new Worker
            {
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                PhoneNumber = dto.PhoneNumber.Trim(),
                BirthDate = birthDate
            };

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync(); // WorkerNumber is from the DB here

            await _auditLogService.LogAsync(
                User,
                "CreateWorker",
                $"Created worker '{worker.NameEn}' / '{worker.NameAr}' (Id: {worker.Id}, Number: {worker.WorkerNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(ToResponseDto(worker));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateWorkerDto dto)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker is null)
            {
                return NotFound(new { message = "Worker not found" });
            }

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) worker.NameEn = nameEn;
            if (nameAr != null) worker.NameAr = nameAr;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                worker.PhoneNumber = dto.PhoneNumber.Trim();

            if (dto.BirthDate.HasValue)
            {
                if (dto.BirthDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
                    return BadRequest(new { message = "Birth date cannot be in the future" });
                worker.BirthDate = dto.BirthDate.Value;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                "UpdateWorker",
                $"Updated worker (Id: {worker.Id}, Number: {worker.WorkerNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(ToResponseDto(worker));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Workers.OrderBy(w => w.Id);
            var totalCount = await query.CountAsync();

            var workers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new WorkerResponseDto
                {
                    Id = w.Id,
                    WorkerNumber = w.WorkerNumber,
                    NameEn = w.NameEn,
                    NameAr = w.NameAr,
                    PhoneNumber = w.PhoneNumber,
                    BirthDate = w.BirthDate,
                    IsActive = w.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<WorkerResponseDto>
            {
                Items = workers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker is null)
            {
                return NotFound(new { message = "Worker not found" });
            }

            worker.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateWorker" : "DeactivateWorker",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} worker '{worker.NameEn}' / '{worker.NameAr}' (Id: {worker.Id}, Number: {worker.WorkerNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Worker {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker is null)
            {
                return NotFound(new { message = "Worker not found" });
            }

            _context.Workers.Remove(worker);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(worker);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteWorker",
                $"Deleted worker '{worker.NameEn}' / '{worker.NameAr}' (Id: {worker.Id}, Number: {worker.WorkerNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Worker deleted successfully" });
        }

        private static WorkerResponseDto ToResponseDto(Worker worker) => new()
        {
            Id = worker.Id,
            WorkerNumber = worker.WorkerNumber,
            NameEn = worker.NameEn,
            NameAr = worker.NameAr,
            PhoneNumber = worker.PhoneNumber,
            BirthDate = worker.BirthDate,
            IsActive = worker.IsActive
        };
    }
}