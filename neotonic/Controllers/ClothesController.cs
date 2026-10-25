using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;
using System.Reflection.PortableExecutable;

namespace neotonic.Controllers
{
    [Route("api/clothes")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ClothesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public ClothesController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateClotheDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();
            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }
            var clothe = new Clothe
            {
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };
            _context.Clothes.Add(clothe);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(
                User,
                "CreateClothe",
                $"Created clothe '{clothe.NameEn}' / '{clothe.NameAr}' (Id: {clothe.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(new ClotheResponseDto
            {
                Id = clothe.Id,
                NameEn = clothe.NameEn,
                NameAr = clothe.NameAr,
                Notes = clothe.Notes,
                IsActive = clothe.IsActive
            });
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateClotheDto dto)
        {
            var clothe = await _context.Clothes.FindAsync(id);
            if (clothe is null)
            {
                return NotFound(new { message = "Clothe does not exist" });
            }

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) clothe.NameEn = nameEn;
            if (nameAr != null) clothe.NameAr = nameAr;

            if (dto.Notes != null)
                clothe.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(
                User,
                "UpdateClothe",
                $"Updated clothe '{clothe.NameEn}' / '{clothe.NameAr}' (Id: {clothe.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(new ClotheResponseDto
            {
                Id = clothe.Id,
                NameEn = clothe.NameEn,
                NameAr = clothe.NameAr,
                Notes = clothe.Notes,
                IsActive = clothe.IsActive
            });
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Clothes.OrderBy(s => s.Id);
            var totalCount = await query.CountAsync();

            var clothes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ClotheResponseDto
                {
                    Id = s.Id,
                    NameEn = s.NameEn,
                    NameAr = s.NameAr,
                    Notes = s.Notes,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<ClotheResponseDto>
            {
                Items = clothes,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var clothe = await _context.Clothes.FindAsync(id);
            if (clothe == null)
            {
                return NotFound(new { message = "Clothe type not found" });
            }

            clothe.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateClothe" : "DeactivateClothe",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} clothe '{clothe.NameEn}' / '{clothe.NameAr}' (Id: {clothe.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"clothe {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clothe = await _context.Clothes.FindAsync(id);
            if (clothe == null)
            {
                return NotFound(new { message = "Clothe not found" });
            }


               _context.Clothes.Remove(clothe);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(clothe);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteClothe",
                $"Deleted clothe '{clothe.NameEn}' / '{clothe.NameAr}' (Id: {clothe.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "clothe deleted successfully" });
        }
    }
}