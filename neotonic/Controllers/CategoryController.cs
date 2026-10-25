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
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace neotonic.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public CategoryController(AppDbContext context, IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
            _context = context;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            if (!BilingualFieldResolver.TryResolve(dto.NameEn, dto.NameAr, out var enName, out var arName))
            {
                return BadRequest(new { message = "At least one of EnName or ArName must be provided" });
            }

            var category = new Category
            {
                NameEn = enName,
                NameAr = arName
            };
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A category with this name already exists" });
            }
            await _auditLogService.LogAsync(
    User,
    "CreateCategory",
    $"Created category '{category.NameEn}' / '{category.NameAr}' (Id: {category.Id})",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(category);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(UpdateCategoryDto dto,int id)
        {
            if (string.IsNullOrWhiteSpace(dto.NameEn)) dto.NameEn = null;
            if (string.IsNullOrWhiteSpace(dto.NameAr)) dto.NameAr = null;
            if (dto.NameEn == null && dto.NameAr == null)
            {
                return BadRequest(new { message = "At least one of EnName or ArName must be provided" });
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }
            var oldEname = category.NameEn;
            var oldAname = category.NameAr;
            if (dto.NameEn != null)
            {
                category.NameEn = dto.NameEn;
            }
            if (dto.NameAr != null)
            {
                category.NameAr = dto.NameAr;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A category with this name already exists" });
            }
            await _auditLogService.LogAsync(
    User,
    "UpdateCategory",
    $"Updated category from '{oldEname}' to '{category.NameEn}' / and from '{oldAname}' to '{category.NameAr}' (Id: {category.Id})",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(category);
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Categories.OrderBy(c => c.Id);

            var totalCount = await query.CountAsync();

            var categories = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    IsActive=c.IsActive,
                })
                .ToListAsync();

            var result = new PagedResultDto<CategoryResponseDto>
            {
                Items = categories,
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
            // 1. Find the target category
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }
            _context.Categories.Remove(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Simply call the helper method here!
                var conflictPayload =await _context.GetDeleteConflictsAsync(category);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteCategory",
                $"Deleted category '{category.NameEn}' / '{category.NameAr}' (Id: {category.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Category deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            category.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(
     User,
     dto.IsActive ? "ActivateCategory" : "DeactivateCategory",
     $"{(dto.IsActive ? "Activated" : "Deactivated")} category '{category.NameEn}' / '{category.NameAr}' (Id: {category.Id})",
     HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Category {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

    }
}
