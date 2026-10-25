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

namespace neotonic.Controllers
{
    [Route("api/materials")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class MaterialController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public MaterialController(AppDbContext context , IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateMaterialDto dto)
        {
            // Trim first so " Steel" can't slip past the unique indexes as a near-duplicate
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var min = dto.Min!.Value;
            var max = dto.Max!.Value;
            if (max < min)
            {
                return BadRequest(new { message = "Max must be greater than or equal to Min" });
            }

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category is null || !category.IsActive)
            {
                return BadRequest(new { message = "Category does not exist or is inactive" });
            }

            var materialType = await _context.MaterialTypes.FindAsync(dto.MaterialTypeId);
            if (materialType is null || !materialType.IsActive)
            {
                return BadRequest(new { message = "Material type does not exist or is inactive" });
            }

            var material = new Material
            {
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                MaterialNumber = dto.MaterialNumber.Trim(),
                AccountingCode = dto.AccountingCode.Trim(),
                Min = min,
                Max = max,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                CategoryId = category.Id,
                MaterialTypeId = materialType.Id,
                IsSerialized=dto.IsSerialized
            };

            try
            {
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                var field =
                    sqlEx.Message.Contains("ix_materials_material_number") ? "material number" :
                    sqlEx.Message.Contains("ix_materials_accounting_code") ? "accounting code" :
                    sqlEx.Message.Contains("ix_materials_name_en") ? "English name" :
                    sqlEx.Message.Contains("ix_materials_name_ar") ? "Arabic name" :
                    "value";

                return Conflict(new { message = $"A material with this {field} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateMaterial",
                $"Created material '{material.NameEn}' / '{material.NameAr}' (Id: {material.Id}, Number: {material.MaterialNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new MaterialResponseDto
            {
                Id = material.Id,
                NameEn = material.NameEn,
                NameAr = material.NameAr,
                MaterialNumber = material.MaterialNumber,
                AccountingCode = material.AccountingCode,
                Min = material.Min,
                Max = material.Max,
                Notes = material.Notes,
                CategoryId = category.Id,
                CategoryNameEn = category.NameEn,
                CategoryNameAr = category.NameAr,
                MaterialTypeId = materialType.Id,
                MaterialTypeNameEn = materialType.NameEn,
                MaterialTypeNameAr = materialType.NameAr,
                IsActive = material.IsActive,
                IsSerialized = material.IsSerialized,
            });
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateMaterialDto dto)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material is null)
            {
                return NotFound(new { message = "Material not found" });
            }

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();

            // Only touch the bilingual fields if at least one was actually sent.
            // If neither was sent, leave both names as they are — this is a partial update.
            if (nameEn != null || nameAr != null)
            {
                var currentEn = nameEn ?? material.NameEn;
                var currentAr = nameAr ?? material.NameAr;
                material.NameEn = currentEn;
                material.NameAr = currentAr;
            }

            if (!string.IsNullOrWhiteSpace(dto.MaterialNumber))
                material.MaterialNumber = dto.MaterialNumber.Trim();

            if (!string.IsNullOrWhiteSpace(dto.AccountingCode))
                material.AccountingCode = dto.AccountingCode.Trim();

            if (dto.Min.HasValue)
                material.Min = dto.Min.Value;

            if (dto.Max.HasValue)
                material.Max = dto.Max.Value;

            if (material.Max < material.Min)
            {
                return BadRequest(new { message = "Max must be greater than or equal to Min" });
            }

            if (dto.Notes != null)
                material.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category is null || !category.IsActive)
                    return BadRequest(new { message = "Category does not exist or is inactive" });
                material.CategoryId = category.Id;
            }

            if (dto.MaterialTypeId.HasValue)
            {
                var materialType = await _context.MaterialTypes.FindAsync(dto.MaterialTypeId.Value);
                if (materialType is null || !materialType.IsActive)
                    return BadRequest(new { message = "Material type does not exist or is inactive" });
                material.MaterialTypeId = materialType.Id;
            }
            if (dto.IsSerialized.HasValue)
            {
                material.IsSerialized = dto.IsSerialized.Value;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                var field =
                    sqlEx.Message.Contains("ix_materials_material_number") ? "material number" :
                    sqlEx.Message.Contains("ix_materials_accounting_code") ? "accounting code" :
                    sqlEx.Message.Contains("ix_materials_name_en") ? "English name" :
                    sqlEx.Message.Contains("ix_materials_name_ar") ? "Arabic name" :
                    "value";

                return Conflict(new { message = $"A material with this {field} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateMaterial",
                $"Updated material (Id: {material.Id}, Number: {material.MaterialNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(material));
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Materials
                .Include(m => m.Category)
                .Include(m => m.MaterialType)
                .OrderBy(m => m.Id);

            var totalCount = await query.CountAsync();

            var materials = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MaterialResponseDto
                {
                    Id = m.Id,
                    NameEn = m.NameEn,
                    NameAr = m.NameAr,
                    MaterialNumber = m.MaterialNumber,
                    AccountingCode = m.AccountingCode,
                    Min = m.Min,
                    Max = m.Max,
                    Notes = m.Notes,
                    CategoryId = m.CategoryId,
                    CategoryNameEn = m.Category.NameEn,
                    CategoryNameAr = m.Category.NameAr,
                    MaterialTypeId = m.MaterialTypeId,
                    MaterialTypeNameEn = m.MaterialType.NameEn,
                    MaterialTypeNameAr = m.MaterialType.NameAr,
                    IsSerialized = m.IsSerialized,
                    IsActive = m.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<MaterialResponseDto>
            {
                Items = materials,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material is null)
            {
                return NotFound(new { message = "Material not found" });
            }

            _context.Materials.Remove(material);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(material);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteMaterial",
                $"Deleted material '{material.NameEn}' / '{material.NameAr}' (Id: {material.Id}, Number: {material.MaterialNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Material deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material is null)
            {
                return NotFound(new { message = "Material not found" });
            }

            material.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateMaterial" : "DeactivateMaterial",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} material '{material.NameEn}' / '{material.NameAr}' (Id: {material.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Material {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
        private async Task<MaterialResponseDto> ToResponseDto(Material material)
        {
            await _context.Entry(material).Reference(m => m.Category).LoadAsync();
            await _context.Entry(material).Reference(m => m.MaterialType).LoadAsync();

            return new MaterialResponseDto
            {
                Id = material.Id,
                NameEn = material.NameEn,
                NameAr = material.NameAr,
                MaterialNumber = material.MaterialNumber,
                AccountingCode = material.AccountingCode,
                Min = material.Min,
                Max = material.Max,
                Notes = material.Notes,
                CategoryId = material.CategoryId,
                CategoryNameEn = material.Category.NameEn,
                CategoryNameAr = material.Category.NameAr,
                MaterialTypeId = material.MaterialTypeId,
                MaterialTypeNameEn = material.MaterialType.NameEn,
                MaterialTypeNameAr = material.MaterialType.NameAr,
                IsSerialized=material.IsSerialized,
                IsActive = material.IsActive
            };
        }
    }
}
