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
    [Route("api/releaseorders")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ReleaseOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITransferOrderService _transfer;
        private readonly IAuditLogService _audit;

        public ReleaseOrdersController(AppDbContext context, ITransferOrderService transfer, IAuditLogService audit)
        {
            _context = context;
            _transfer = transfer;
            _audit = audit;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateReleaseOrderDto dto)
        {
            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.Id == dto.MaterialId);
            if (material is null || !material.IsActive)
                return BadRequest(new { message = "Material does not exist or is inactive" });
            if (material.MaterialType.FlowId is null)
                return BadRequest(new { message = "Material type has no flow configured" });

            var producedInvId = InventoryIds.ProductsProduced;

            var steps = await _context.FlowSteps
                .Where(s => s.FlowId == material.MaterialType.FlowId.Value)
                .OrderBy(s => s.OrderIndex)
                .Include(s => s.Location)
                .ToListAsync();

            var producedStep = steps.FirstOrDefault(s => s.Location.InventoryId == producedInvId);
            if (producedStep is null)
                return BadRequest(new { message = "This material's flow does not include Products-produced" });

            var previousStep = steps.LastOrDefault(s => s.OrderIndex < producedStep.OrderIndex);
            if (previousStep?.Location.InventoryId is null)
                return BadRequest(new { message = "Products-produced is the first step of this material's flow; release is not possible" });

            var fromLocation = await _context.Locations
                .FirstOrDefaultAsync(l => l.InventoryId == previousStep.Location.InventoryId && l.Zone == InventoryZones.Accepted);
            if (fromLocation is null)
                return BadRequest(new { message = "Source inventory has no Accepted zone" });

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _transfer.CreateAsync(
                    fromLocation.Id,
                    LocationIds.ProductionFloor,
                    material.Id,
                    dto.Quantity,
                    allowOffFlowDestination: true);

                var order = new ReleaseOrder
                {
                    Number = dto.Number.Trim(),
                    OrderDate = dto.OrderDate ?? DateTime.UtcNow,
                    ManufacturingOrderId = dto.ManufacturingOrderId,
                    Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                    TransferOrderId = transfer.Id
                };
                _context.ReleaseOrders.Add(order);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _audit.LogAsync(User, "CreateReleaseOrder",
                    $"Created release order '{order.Number}' (Id: {order.Id}, TransferOrder: {transfer.Id})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(await ToResponseDto(order.Id));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "A release order with this number already exists" });
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateReleaseOrderDto dto)
        {
            var order = await _context.ReleaseOrders
                .Include(o => o.TransferOrder)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Release order not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number)) order.Number = dto.Number.Trim();
            if (dto.OrderDate.HasValue) order.OrderDate = dto.OrderDate.Value;
            if (dto.ManufacturingOrderId.HasValue) order.ManufacturingOrderId = dto.ManufacturingOrderId.Value;
            if (dto.Notes != null)
                order.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A release order with this number already exists" });
            }

            await _audit.LogAsync(User, "UpdateReleaseOrder",
                $"Updated release order (Id: {order.Id}, Number: {order.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpPut("{id}/lines")]
        public async Task<IActionResult> SetLines(int id, SetTransferLinesDto dto)
        {
            var order = await _context.ReleaseOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Release order not found" });
            if (order.TransferOrderId is null) return BadRequest(new { message = "Order has no transfer order" });

            try
            {
                await _transfer.SetLinesAsync(order.TransferOrderId.Value,
                    dto.Lines.Select(l => (l.UomId, l.Quantity)));
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "SetReleaseOrderLines",
                $"Set {dto.Lines.Count} line(s) on release order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpPut("{id}/done")]
        public async Task<IActionResult> MarkDone(int id)
        {
            var order = await _context.ReleaseOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Release order not found" });

            try { await _transfer.MarkDoneAsync(order.TransferOrderId.Value); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "MarkReleaseOrderDone",
                $"Marked release order '{order.Number}' Done (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.ReleaseOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Release order not found" });

            try { await _transfer.CancelAsync(order.TransferOrderId.Value); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "CancelReleaseOrder",
                $"Cancelled release order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpPut("{id}/finish")]
        public async Task<IActionResult> Finish(int id, FinishTransferOrderDto dto)
        {
            var order = await _context.ReleaseOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Release order not found" });

            try { await _transfer.FinishAsync(order.TransferOrderId.Value, dto.ScannedUomIds); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "FinishReleaseOrder",
                $"Finished release order '{order.Number}' (Id: {order.Id}, scanned {dto.ScannedUomIds.Count})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpGet("{id}/candidates")]
        public async Task<IActionResult> Candidates(int id)
        {
            var order = await _context.ReleaseOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Release order not found" });
            return Ok(await _transfer.GetCandidatesAsync(order.TransferOrderId.Value));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ReleaseOrders
                .Include(o => o.TransferOrder!).ThenInclude(t => t.FromLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.ToLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Material)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Lines).ThenInclude(l => l.Uom)
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id);

            var totalCount = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new PagedResultDto<ReleaseOrderResponseDto>
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
            var exists = await _context.ReleaseOrders.AnyAsync(o => o.Id == id);
            if (!exists) return NotFound(new { message = "Release order not found" });
            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var order = await _context.ReleaseOrders.FindAsync(id);
            if (order is null) return NotFound(new { message = "Release order not found" });

            order.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _audit.LogAsync(User, dto.IsActive ? "ActivateReleaseOrder" : "DeactivateReleaseOrder",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} release order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Release order {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.ReleaseOrders
                .Include(o => o.TransferOrder)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Release order not found" });

            if (order.TransferOrder is not null
                && order.TransferOrder.Status is not (TransferOrderStatuses.New or TransferOrderStatuses.Cancelled))
                return Conflict(new { message = "Cannot delete: transfer order has already been Done or Executed" });

            _context.ReleaseOrders.Remove(order);
            if (order.TransferOrder is not null) _context.TransferOrders.Remove(order.TransferOrder);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(order);
                return Conflict(conflictPayload);
            }

            await _audit.LogAsync(User, "DeleteReleaseOrder",
                $"Deleted release order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Release order deleted successfully" });
        }

        private async Task<ReleaseOrderResponseDto> ToResponseDto(int id)
        {
            var order = await _context.ReleaseOrders
                .Include(o => o.TransferOrder!).ThenInclude(t => t.FromLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.ToLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Material)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Lines).ThenInclude(l => l.Uom).ThenInclude(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .FirstAsync(o => o.Id == id);
            return ToDto(order);
        }

        private static ReleaseOrderResponseDto ToDto(ReleaseOrder o)
        {
            return new ReleaseOrderResponseDto
            {
                Id = o.Id,
                Number = o.Number,
                OrderDate = o.OrderDate,
                ManufacturingOrderId = o.ManufacturingOrderId,
                Notes = o.Notes,
                IsActive = o.IsActive,
                TransferOrder = o.TransferOrder is null ? null : ToTransferDto(o.TransferOrder)
            };
        }

        private static TransferOrderResponseDto ToTransferDto(TransferOrder t)
        {
            var (statusAr, statusEn) = t.Status.GetBilingualNames();
            return new TransferOrderResponseDto
            {
                Id = t.Id,
                FromLocationId = t.FromLocationId,
                FromLocationCode = t.FromLocation?.Code ?? string.Empty,
                ToLocationId = t.ToLocationId,
                ToLocationCode = t.ToLocation?.Code ?? string.Empty,
                MaterialId = t.MaterialId,
                MaterialNumber = t.Material?.MaterialNumber ?? string.Empty,
                MaterialNameEn = t.Material?.NameEn ?? string.Empty,
                MaterialNameAr = t.Material?.NameAr ?? string.Empty,
                Quantity = t.Quantity,
                SatisfiedQuantity = t.Lines.Sum(l => l.Quantity),
                Status = t.Status,
                StatusNameEn = statusEn,
                StatusNameAr = statusAr,
                ExecutedAt = t.ExecutedAt,
                Lines = t.Lines.Select(l => new TransferOrderLineResponseDto
                {
                    Id = l.Id,
                    UomId = l.UomId,
                    UomBarcode = l.Uom?.Barcode ?? string.Empty,
                    BoxQuantity = l.Uom?.Quantity ?? 0m,
                    LineQuantity = l.Quantity,
                    IsScanned = l.IsScanned,
                    ShelfId = l.Uom?.ShelfId,
                    ShelfLabel = l.Uom?.Shelf is null ? null : Label(l.Uom.Shelf)
                }).ToList()
            };
        }

        private static string Label(Shelf s) =>
            $"{s.Location.Inventory!.Code}-{(s.Location.Zone switch { InventoryZones.Quarantine => "A", InventoryZones.Accepted => "B", _ => "C" })}{s.Number}";
    }
}