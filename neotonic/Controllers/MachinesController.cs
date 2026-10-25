using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Helpers;

//using neotonic.Migrations;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/machines")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class MachinesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public MachinesController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateMachineDto dto)
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

            var machine = new Machine
            {
                MachineNumber = dto.MachineNumber.Trim(),
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                DepartmentId = department.Id
            };

            try
            {
                _context.Machines.Add(machine);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A machine with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateMachine",
                $"Created machine '{machine.NameEn}' / '{machine.NameAr}' (Id: {machine.Id}, Number: {machine.MachineNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(machine));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateMachineDto dto)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine is null)
            {
                return NotFound(new { message = "Machine not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.MachineNumber))
                machine.MachineNumber = dto.MachineNumber.Trim();

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) machine.NameEn = nameEn;
            if (nameAr != null) machine.NameAr = nameAr;

            if (dto.Notes != null)
                machine.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(dto.DepartmentId.Value);
                if (department is null || !department.IsActive)
                    return BadRequest(new { message = "Department does not exist or is inactive" });
                machine.DepartmentId = department.Id;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A machine with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateMachine",
                $"Updated machine (Id: {machine.Id}, Number: {machine.MachineNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(machine));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Machines
                .Include(m => m.Department)
                .OrderBy(m => m.Id);

            var totalCount = await query.CountAsync();

            var machines = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MachineResponseDto
                {
                    Id = m.Id,
                    MachineNumber = m.MachineNumber,
                    NameEn = m.NameEn,
                    NameAr = m.NameAr,
                    Notes = m.Notes,
                    DepartmentId = m.DepartmentId,
                    DepartmentNameEn = m.Department.NameEn,
                    DepartmentNameAr = m.Department.NameAr,
                    IsActive = m.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<MachineResponseDto>
            {
                Items = machines,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine is null)
            {
                return NotFound(new { message = "Machine not found" });
            }

            machine.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateMachine" : "DeactivateMachine",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} machine '{machine.NameEn}' / '{machine.NameAr}' (Id: {machine.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Machine {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine is null)
            {
                return NotFound(new { message = "Machine not found" });
            }

            _context.Machines.Remove(machine);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(machine);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteMachine",
                $"Deleted machine '{machine.NameEn}' / '{machine.NameAr}' (Id: {machine.Id}, Number: {machine.MachineNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Machine deleted successfully" });
        }

        private async Task<MachineResponseDto> ToResponseDto(Machine machine)
        {
            await _context.Entry(machine).Reference(m => m.Department).LoadAsync();

            return new MachineResponseDto
            {
                Id = machine.Id,
                MachineNumber = machine.MachineNumber,
                NameEn = machine.NameEn,
                NameAr = machine.NameAr,
                Notes = machine.Notes,
                DepartmentId = machine.DepartmentId,
                DepartmentNameEn = machine.Department.NameEn,
                DepartmentNameAr = machine.Department.NameAr,
                IsActive = machine.IsActive
            };
        }

        private static string FieldFromIndex(string sqlMessage) =>
            sqlMessage.Contains("ix_machines_machine_number") ? "machine number" :
            sqlMessage.Contains("ix_machines_name_en") ? "English name" :
            sqlMessage.Contains("ix_machines_name_ar") ? "Arabic name" :
            "value";
    }
}