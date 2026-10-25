using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/labequipments")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class LabEquipmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public LabEquipmentsController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateLabEquipmentDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();
            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedNameEn, out var resolvedNameAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var jobEn = dto.JobEn?.Trim();
            var jobAr = dto.JobAr?.Trim();
            if (!BilingualFieldResolver.TryResolve(jobEn, jobAr, out var resolvedJobEn, out var resolvedJobAr))
            {
                return BadRequest(new { message = "At least one of English Job or Arabic Job must be provided" });
            }

            var equipment = new LabEquipment
            {
                NameEn = resolvedNameEn,
                NameAr = resolvedNameAr,
                JobEn = resolvedJobEn,
                JobAr = resolvedJobAr,
                Manufacturer = dto.Manufacturer.Trim(),
                Model = dto.Model.Trim(),
                EqualizingDate = dto.EqualizingDate!.Value
            };

            _context.LabEquipments.Add(equipment);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                "CreateLabEquipment",
                $"Created lab equipment '{equipment.NameEn}' / '{equipment.NameAr}' (Id: {equipment.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(ToResponseDto(equipment));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateLabEquipmentDto dto)
        {
            var equipment = await _context.LabEquipments.FindAsync(id);
            if (equipment is null)
            {
                return NotFound(new { message = "Lab equipment not found" });
            }

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) equipment.NameEn = nameEn;
            if (nameAr != null) equipment.NameAr = nameAr;

            var jobEn = string.IsNullOrWhiteSpace(dto.JobEn) ? null : dto.JobEn.Trim();
            var jobAr = string.IsNullOrWhiteSpace(dto.JobAr) ? null : dto.JobAr.Trim();
            if (jobEn != null) equipment.JobEn = jobEn;
            if (jobAr != null) equipment.JobAr = jobAr;

            if (!string.IsNullOrWhiteSpace(dto.Manufacturer))
                equipment.Manufacturer = dto.Manufacturer.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Model))
                equipment.Model = dto.Model.Trim();

            if (dto.EqualizingDate.HasValue)
                equipment.EqualizingDate = dto.EqualizingDate.Value;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                "UpdateLabEquipment",
                $"Updated lab equipment (Id: {equipment.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(ToResponseDto(equipment));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.LabEquipments.OrderBy(l => l.Id);
            var totalCount = await query.CountAsync();

            var equipments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LabEquipmentResponseDto
                {
                    Id = l.Id,
                    NameEn = l.NameEn,
                    NameAr = l.NameAr,
                    JobEn = l.JobEn,
                    JobAr = l.JobAr,
                    Manufacturer = l.Manufacturer,
                    Model = l.Model,
                    EqualizingDate = l.EqualizingDate,
                    IsActive = l.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<LabEquipmentResponseDto>
            {
                Items = equipments,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var equipment = await _context.LabEquipments.FindAsync(id);
            if (equipment is null)
            {
                return NotFound(new { message = "Lab equipment not found" });
            }

            equipment.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateLabEquipment" : "DeactivateLabEquipment",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} lab equipment '{equipment.NameEn}' / '{equipment.NameAr}' (Id: {equipment.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Lab equipment {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var equipment = await _context.LabEquipments.FindAsync(id);
            if (equipment is null)
            {
                return NotFound(new { message = "Lab equipment not found" });
            }

            _context.LabEquipments.Remove(equipment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(equipment);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteLabEquipment",
                $"Deleted lab equipment '{equipment.NameEn}' / '{equipment.NameAr}' (Id: {equipment.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Lab equipment deleted successfully" });
        }

        private static LabEquipmentResponseDto ToResponseDto(LabEquipment equipment) => new()
        {
            Id = equipment.Id,
            NameEn = equipment.NameEn,
            NameAr = equipment.NameAr,
            JobEn = equipment.JobEn,
            JobAr = equipment.JobAr,
            Manufacturer = equipment.Manufacturer,
            Model = equipment.Model,
            EqualizingDate = equipment.EqualizingDate,
            IsActive = equipment.IsActive
        };
    }
}