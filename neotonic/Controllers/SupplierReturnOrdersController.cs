using Microsoft.AspNetCore.Authorization;
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
    [Route("api/supplierreturnorders")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class SupplierReturnOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITransferOrderService _transfer;
        private readonly IAuditLogService _audit;

        public SupplierReturnOrdersController(AppDbContext context, ITransferOrderService transfer, IAuditLogService audit)
        {
            _context = context;
            _transfer = transfer;
            _audit = audit;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSupplierReturnOrderDto dto)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == dto.SupplierId);
            if (supplier is null || !supplier.IsActive)
                return BadRequest(new { message = "Supplier does not exist or is inactive" });

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.Id == dto.MaterialId);
            if (material is null || !material.IsActive)
                return BadRequest(new { message = "Material does not exist or is inactive" });
            if (material.MaterialType.FlowId is null)
                return BadRequest(new { message = "Material type has no flow configured" });

            var from = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == dto.FromLocationId);
            if (from is null || !from.IsActive)
                return BadRequest(new { message = "Source location does not exist or is inactive" });
            if (from.Id == LocationIds.SupplierReturns)
                return BadRequest(new { message = "Source cannot be the supplier return location" });

            // Must be an entry location (first FlowStep of the material's flow).
            var firstStep = await _context.FlowSteps
                .Where(s => s.FlowId == material.MaterialType.FlowId.Value)
                .OrderBy(s => s.OrderIndex)
                .FirstOrDefaultAsync();
            if (firstStep is null)
                return BadRequest(new { message = "Material type's flow has no steps" });
            if (firstStep.LocationId != from.Id)
                return BadRequest(new { message = "Source must be the entry location of this material's flow" });

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _transfer.CreateAsync(
                    from.Id, LocationIds.SupplierReturns, material.Id, dto.Quantity,
                    allowOffFlowDestination: true);

                var order = new SupplierReturnOrder
                {
                    Number = dto.Number.Trim(),
                    OrderDate = dto.OrderDate ?? DateTime.UtcNow,
                    SupplierId = supplier.Id,
                    Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                    TransferOrderId = transfer.Id
                };
                _context.SupplierReturnOrders.Add(order);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _audit.LogAsync(User, "CreateSupplierReturnOrder",
                    $"Created supplier return order '{order.Number}' (Id: {order.Id}, Supplier: {supplier.SupplierNumber}, TransferOrder: {transfer.Id})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(await ToResponseDto(order.Id));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "A supplier return order with this number already exists" });
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateSupplierReturnOrderDto dto)
        {
            var order = await _context.SupplierReturnOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Supplier return order not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number)) order.Number = dto.Number.Trim();
            if (dto.OrderDate.HasValue) order.OrderDate = dto.OrderDate.Value;
            if (dto.Notes != null)
                order.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A supplier return order with this number already exists" });
            }

            await _audit.LogAsync(User, "UpdateSupplierReturnOrder",
                $"Updated supplier return order (Id: {order.Id}, Number: {order.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        // Lines must all belong to the order's supplier.
        [HttpPut("{id}/lines")]
        public async Task<IActionResult> SetLines(int id, SetTransferLinesDto dto)
        {
            var order = await _context.SupplierReturnOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Supplier return order not found" });
            if (order.TransferOrderId is null) return BadRequest(new { message = "Order has no transfer order" });

            var uomIds = dto.Lines.Select(l => l.UomId).Distinct().ToList();
            var suppliers = await _context.GetSupplierIdsAsync(uomIds);

            foreach (var uomId in uomIds)
            {
                var supplierId = suppliers.GetValueOrDefault(uomId);
                if (supplierId is null)
                    return BadRequest(new { message = $"Supplier for box {uomId} could not be determined; it cannot be returned" });
                if (supplierId != order.SupplierId)
                    return BadRequest(new { message = $"Box {uomId} does not belong to supplier {order.SupplierId}; only boxes of the same supplier can be returned" });
            }

            try
            {
                await _transfer.SetLinesAsync(order.TransferOrderId.Value,
                    dto.Lines.Select(l => (l.UomId, l.Quantity)));
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "SetSupplierReturnOrderLines",
                $"Set {dto.Lines.Count} line(s) on supplier return order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        // Candidate list: boxes in FromLocation, material matches, and supplier matches the order's SupplierId.
        [HttpGet("{id}/candidates")]
        public async Task<IActionResult> Candidates(int id)
        {
            var order = await _context.SupplierReturnOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Supplier return order not found" });

            var all = await _transfer.GetCandidatesAsync(order.TransferOrderId.Value);
            if (all.Count == 0) return Ok(all);

            var suppliers = await _context.GetSupplierIdsAsync(all.Select(c => c.Id));
            var filtered = all
                .Where(c => suppliers.GetValueOrDefault(c.Id) == order.SupplierId)
                .ToList();

            return Ok(filtered);
        }

        [HttpPut("{id}/done")]
        public async Task<IActionResult> MarkDone(int id)
        {
            var order = await _context.SupplierReturnOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Supplier return order not found" });

            try { await _transfer.MarkDoneAsync(order.TransferOrderId.Value); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "MarkSupplierReturnOrderDone",
                $"Marked supplier return order '{order.Number}' Done (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.SupplierReturnOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Supplier return order not found" });

            try { await _transfer.CancelAsync(order.TransferOrderId.Value); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "CancelSupplierReturnOrder",
                $"Cancelled supplier return order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpPut("{id}/finish")]
        public async Task<IActionResult> Finish(int id, FinishTransferOrderDto dto)
        {
            var order = await _context.SupplierReturnOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Supplier return order not found" });

            try { await _transfer.FinishAsync(order.TransferOrderId.Value, dto.ScannedUomIds); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "FinishSupplierReturnOrder",
                $"Finished supplier return order '{order.Number}' (Id: {order.Id}, scanned {dto.ScannedUomIds.Count})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.SupplierReturnOrders
                .Include(o => o.Supplier)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.FromLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.ToLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Material)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Lines).ThenInclude(l => l.Uom)
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id);

            var totalCount = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new PagedResultDto<SupplierReturnOrderResponseDto>
            {
                Items = orders.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var exists = await _context.SupplierReturnOrders.AnyAsync(o => o.Id == id);
            if (!exists) return NotFound(new { message = "Supplier return order not found" });
            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var order = await _context.SupplierReturnOrders.FindAsync(id);
            if (order is null) return NotFound(new { message = "Supplier return order not found" });

            order.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _audit.LogAsync(User, dto.IsActive ? "ActivateSupplierReturnOrder" : "DeactivateSupplierReturnOrder",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} supplier return order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Supplier return order {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.SupplierReturnOrders
                .Include(o => o.TransferOrder)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Supplier return order not found" });

            if (order.TransferOrder is not null
                && order.TransferOrder.Status is not (TransferOrderStatuses.New or TransferOrderStatuses.Cancelled))
                return Conflict(new { message = "Cannot delete: transfer order has already been Done or Executed" });

            _context.SupplierReturnOrders.Remove(order);
            if (order.TransferOrder is not null) _context.TransferOrders.Remove(order.TransferOrder);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(order);
                return Conflict(conflictPayload);
            }

            await _audit.LogAsync(User, "DeleteSupplierReturnOrder",
                $"Deleted supplier return order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Supplier return order deleted successfully" });
        }

        private async Task<SupplierReturnOrderResponseDto> ToResponseDto(int id)
        {
            var order = await _context.SupplierReturnOrders
                .Include(o => o.Supplier)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.FromLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.ToLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Material)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Lines).ThenInclude(l => l.Uom).ThenInclude(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .FirstAsync(o => o.Id == id);
            return ToDto(order);
        }

        private static SupplierReturnOrderResponseDto ToDto(SupplierReturnOrder o) => new()
        {
            Id = o.Id,
            Number = o.Number,
            OrderDate = o.OrderDate,
            SupplierId = o.SupplierId,
            SupplierNumber = o.Supplier.SupplierNumber,
            SupplierNameEn = o.Supplier.NameEn,
            SupplierNameAr = o.Supplier.NameAr,
            Notes = o.Notes,
            IsActive = o.IsActive,
            TransferOrder = o.TransferOrder is null ? null : TransferDtoMapper.ToDto(o.TransferOrder)
        };
    }
}