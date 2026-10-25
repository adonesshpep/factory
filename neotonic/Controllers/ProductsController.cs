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
    [Route("api/products")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public ProductsController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            // Trim first so " Steel" can't slip past the unique indexes as a near-duplicate
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }


            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category is null || !category.IsActive)
            {
                return BadRequest(new { message = "Category does not exist or is inactive" });
            }


            var product = new Product
            {
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                ProductNumber = dto.ProductNumber.Trim(),
                AccountingCode = dto.AccountingCode.Trim(),
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                CategoryId = category.Id,
                IsSerialized = dto.IsSerialized
            };

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                var field =
                    sqlEx.Message.Contains("ix_products_product_number") ? "product number" :
                    sqlEx.Message.Contains("ix_products_accounting_code") ? "accounting code" :
                    sqlEx.Message.Contains("ix_products_name_en") ? "English name" :
                    sqlEx.Message.Contains("ix_products_name_ar") ? "Arabic name" :
                    "value";

                return Conflict(new { message = $"A product with this {field} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateProduct",
                $"Created product '{product.NameEn}' / '{product.NameAr}' (Id: {product.Id}, Number: {product.ProductNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new ProductResponseDto
            {
                Id = product.Id,
                NameEn = product.NameEn,
                NameAr = product.NameAr,
                ProductNumber = product.ProductNumber,
                AccountingCode = product.AccountingCode,
                Notes = product.Notes,
                CategoryId = category.Id,
                CategoryNameEn = category.NameEn,
                CategoryNameAr = category.NameAr,
                IsActive = product.IsActive,
                IsSerialized = product.IsSerialized,
            });
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
            {
                return NotFound(new { message = "Product not found" });
            }

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();

            // Only touch the bilingual fields if at least one was actually sent.
            // If neither was sent, leave both names as they are — this is a partial update.
            if (nameEn != null || nameAr != null)
            {
                var currentEn = nameEn ?? product.NameEn;
                var currentAr = nameAr ?? product.NameAr;
                product.NameEn = currentEn;
                product.NameAr = currentAr;
            }

            if (!string.IsNullOrWhiteSpace(dto.ProductNumber))
                product.ProductNumber = dto.ProductNumber.Trim();

            if (!string.IsNullOrWhiteSpace(dto.AccountingCode))
                product.AccountingCode = dto.AccountingCode.Trim();



            if (dto.Notes != null)
                product.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category is null || !category.IsActive)
                    return BadRequest(new { message = "Category does not exist or is inactive" });
                product.CategoryId = category.Id;
            }


            if (dto.IsSerialized.HasValue)
            {
                product.IsSerialized = dto.IsSerialized.Value;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                var field =
                    sqlEx.Message.Contains("ix_products_product_number") ? "product number" :
                    sqlEx.Message.Contains("ix_products_accounting_code") ? "accounting code" :
                    sqlEx.Message.Contains("ix_products_name_en") ? "English name" :
                    sqlEx.Message.Contains("ix_products_name_ar") ? "Arabic name" :
                    "value";

                return Conflict(new { message = $"A product with this {field} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateProduct",
                $"Updated product (Id: {product.Id}, Number: {product.ProductNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(product));
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Products
                .Include(m => m.Category)
                .OrderBy(m => m.Id);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ProductResponseDto
                {
                    Id = m.Id,
                    NameEn = m.NameEn,
                    NameAr = m.NameAr,
                    ProductNumber = m.ProductNumber,
                    AccountingCode = m.AccountingCode,
                    Notes = m.Notes,
                    CategoryId = m.CategoryId,
                    CategoryNameEn = m.Category.NameEn,
                    CategoryNameAr = m.Category.NameAr,
                    IsSerialized = m.IsSerialized,
                    IsActive = m.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<ProductResponseDto>
            {
                Items = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
            {
                return NotFound(new { message = "Product not found" });
            }

            _context.Products.Remove(product);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(product);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteProduct",
                $"Deleted product '{product.NameEn}' / '{product.NameAr}' (Id: {product.Id}, Number: {product.ProductNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Product deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
            {
                return NotFound(new { message = "Product not found" });
            }

            product.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateProduct" : "DeactivateProduct",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} product '{product.NameEn}' / '{product.NameAr}' (Id: {product.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Product {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
        private async Task<ProductResponseDto> ToResponseDto(Product product)
        {
            await _context.Entry(product).Reference(m => m.Category).LoadAsync();

            return new ProductResponseDto
            {
                Id = product.Id,
                NameEn = product.NameEn,
                NameAr = product.NameAr,
                ProductNumber = product.ProductNumber,
                AccountingCode = product.AccountingCode,
                Notes = product.Notes,
                CategoryId = product.CategoryId,
                CategoryNameEn = product.Category.NameEn,
                CategoryNameAr = product.Category.NameAr,
                IsSerialized = product.IsSerialized,
                IsActive = product.IsActive
            };
        }
    }
}
