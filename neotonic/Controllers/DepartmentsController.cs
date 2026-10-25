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
    [Route("api/departments")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public DepartmentsController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var department = new Department
            {
                DepartmentNumber = dto.DepartmentNumber.Trim(),
                NameEn = resolvedEn,
                NameAr = resolvedAr
            };

            try
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A department with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateDepartment",
                $"Created department '{department.NameEn}' / '{department.NameAr}' (Id: {department.Id}, Number: {department.DepartmentNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new DepartmentResponseDto
            {
                Id = department.Id,
                DepartmentNumber = department.DepartmentNumber,
                NameEn = department.NameEn,
                NameAr = department.NameAr,
                IsActive = department.IsActive
            });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateDepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department is null)
            {
                return NotFound(new { message = "Department not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.DepartmentNumber))
                department.DepartmentNumber = dto.DepartmentNumber.Trim();

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) department.NameEn = nameEn;
            if (nameAr != null) department.NameAr = nameAr;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A department with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateDepartment",
                $"Updated department (Id: {department.Id}, Number: {department.DepartmentNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new DepartmentResponseDto
            {
                Id = department.Id,
                DepartmentNumber = department.DepartmentNumber,
                NameEn = department.NameEn,
                NameAr = department.NameAr,
                IsActive = department.IsActive
            });
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Departments.OrderBy(d => d.Id);
            var totalCount = await query.CountAsync();

            var departments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    DepartmentNumber = d.DepartmentNumber,
                    NameEn = d.NameEn,
                    NameAr = d.NameAr,
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<DepartmentResponseDto>
            {
                Items = departments,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department is null)
            {
                return NotFound(new { message = "Department not found" });
            }

            department.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateDepartment" : "DeactivateDepartment",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} department '{department.NameEn}' / '{department.NameAr}' (Id: {department.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Department {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department is null)
            {
                return NotFound(new { message = "Department not found" });
            }

            _context.Departments.Remove(department);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload =await _context.GetDeleteConflictsAsync(department);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteDepartment",
                $"Deleted department '{department.NameEn}' / '{department.NameAr}' (Id: {department.Id}, Number: {department.DepartmentNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Department deleted successfully" });
        }

        private static string FieldFromIndex(string sqlMessage) =>
            sqlMessage.Contains("ix_departments_department_number") ? "department number" :
            sqlMessage.Contains("ix_departments_name_en") ? "English name" :
            sqlMessage.Contains("ix_departments_name_ar") ? "Arabic name" :
            "value";
    }
}
