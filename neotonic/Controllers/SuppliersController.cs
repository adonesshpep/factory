using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/suppliers")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public SuppliersController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSupplierDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }

            var supplier = new Supplier
            {
                SupplierNumber = dto.SupplierNumber.Trim(),
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                Contact = string.IsNullOrWhiteSpace(dto.Contact) ? null : dto.Contact.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            try
            {
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A supplier with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateSupplier",
                $"Created supplier '{supplier.NameEn}' / '{supplier.NameAr}' (Id: {supplier.Id}, Number: {supplier.SupplierNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(ToResponseDto(supplier));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateSupplierDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier is null)
            {
                return NotFound(new { message = "Supplier not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.SupplierNumber))
                supplier.SupplierNumber = dto.SupplierNumber.Trim();

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) supplier.NameEn = nameEn;
            if (nameAr != null) supplier.NameAr = nameAr;

            if (dto.Contact != null)
                supplier.Contact = string.IsNullOrWhiteSpace(dto.Contact) ? null : dto.Contact.Trim();

            if (dto.PhoneNumber != null)
                supplier.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

            if (dto.Email != null)
                supplier.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

            if (dto.Notes != null)
                supplier.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A supplier with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateSupplier",
                $"Updated supplier (Id: {supplier.Id}, Number: {supplier.SupplierNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(ToResponseDto(supplier));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Suppliers.OrderBy(s => s.Id);
            var totalCount = await query.CountAsync();

            var suppliers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierResponseDto
                {
                    Id = s.Id,
                    SupplierNumber = s.SupplierNumber,
                    NameEn = s.NameEn,
                    NameAr = s.NameAr,
                    Contact = s.Contact,
                    PhoneNumber = s.PhoneNumber,
                    Email = s.Email,
                    Notes = s.Notes,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new PagedResultDto<SupplierResponseDto>
            {
                Items = suppliers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier is null)
            {
                return NotFound(new { message = "Supplier not found" });
            }

            supplier.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateSupplier" : "DeactivateSupplier",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} supplier '{supplier.NameEn}' / '{supplier.NameAr}' (Id: {supplier.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Supplier {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier is null)
            {
                return NotFound(new { message = "Supplier not found" });
            }

            _context.Suppliers.Remove(supplier);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(supplier);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteSupplier",
                $"Deleted supplier '{supplier.NameEn}' / '{supplier.NameAr}' (Id: {supplier.Id}, Number: {supplier.SupplierNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Supplier deleted successfully" });
        }

        private static SupplierResponseDto ToResponseDto(Supplier supplier) => new()
        {
            Id = supplier.Id,
            SupplierNumber = supplier.SupplierNumber,
            NameEn = supplier.NameEn,
            NameAr = supplier.NameAr,
            Contact = supplier.Contact,
            PhoneNumber = supplier.PhoneNumber,
            Email = supplier.Email,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive
        };

        private static string FieldFromIndex(string sqlMessage) =>
            sqlMessage.Contains("ix_suppliers_supplier_number") ? "supplier number" :
            sqlMessage.Contains("ix_suppliers_name_en") ? "English name" :
            sqlMessage.Contains("ix_suppliers_name_ar") ? "Arabic name" :
            "value";
    }
}