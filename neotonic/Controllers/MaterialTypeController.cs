using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;
using System.Linq.Expressions;

namespace neotonic.Controllers
{
    [Route("api/materialtypes")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class MaterialTypeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public MaterialTypeController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateMaterialTypeDto dto)
        {
            if (!BilingualFieldResolver.TryResolve(dto.NameEn, dto.NameAr, out var Nameen, out var Namear))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var type = new MaterialType
            {
                NameEn = Nameen,
                NameAr = Namear
            };
            try
            {
                _context.MaterialTypes.Add(type);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A material type with this name already exists" });
            }
            await _auditLogService.LogAsync(
    User,
    "CreateMaterialType",
    $"Created Material Type '{type.NameEn}' / '{type.NameAr}' (Id: {type.Id})",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(type);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(CreateMaterialTypeDto dto, int id)
        {
            if (string.IsNullOrWhiteSpace(dto.NameEn)) dto.NameEn = null;
            if (string.IsNullOrWhiteSpace(dto.NameAr)) dto.NameAr = null;
            if (dto.NameEn == null && dto.NameAr == null)
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }
            var type = await _context.MaterialTypes.FindAsync(id);
            if (type == null)
            {
                return NotFound(new { message = "Material Type not found" });
            }
            var oldEname = type.NameEn;
            var oldAname = type.NameAr;
            if (dto.NameEn != null)
            {
                type.NameEn = dto.NameEn;
            }
            if (dto.NameAr != null)
            {
                type.NameAr = dto.NameAr;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A Material Type with this name already exists" });
            }
            await _auditLogService.LogAsync(
    User,
    "UpdateMaterialType",
    $"Updated Material Type from '{oldEname}' to '{type.NameEn}' / and from '{oldAname}' to '{type.NameAr}' (Id: {type.Id})",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(type);
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.MaterialTypes.OrderBy(c => c.Id);

            var totalCount = await query.CountAsync();

            var types = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new MaterialTypeResponseDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    IsActive = c.IsActive,
                })
                .ToListAsync();

            var result = new PagedResultDto<MaterialTypeResponseDto>
            {
                Items = types,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _context.MaterialTypes.FindAsync(id);
            if (type == null)
            {
                return NotFound(new { message = "Material Type not found" });
            }
            _context.MaterialTypes.Remove(type);


            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(type);
                return Conflict(conflictPayload);
            }
            await _auditLogService.LogAsync(
    User,
    "DeleteMaterialType",
    $"Deleted material type '{type.NameEn}' / '{type.NameAr}' (Id: {type.Id})",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(new { message = "Material Type deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var type = await _context.MaterialTypes.FindAsync(id);
            if (type == null)
            {
                return NotFound(new { message = "Material Type not found" });
            }

            type.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(
     User,
     dto.IsActive ? "ActivateMaterialType" : "DeactivateMaterialType",
     $"{(dto.IsActive ? "Activated" : "Deactivated")} material type '{type.NameEn}' / '{type.NameAr}' (Id: {type.Id})",
     HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Material Type {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

    }
}
