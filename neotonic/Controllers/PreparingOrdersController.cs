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
    [Route("api/preparingorders")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class PreparingOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITransferOrderService _transfer;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notifications;

        public PreparingOrdersController(AppDbContext context, ITransferOrderService transfer, IAuditLogService audit, INotificationService notifications)
        {
            _context = context;
            _transfer = transfer;
            _audit = audit;
            _notifications = notifications;
        }

        [Authorize(Roles = "Admin, Warehouse, Production")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreatePreparingOrderDto dto)
        {
            var protocol = await _context.Protocols
                .Include(p => p.StepType)
                .FirstOrDefaultAsync(p => p.Id == dto.ProtocolId);
            if (protocol is null || !protocol.IsActive)
                return BadRequest(new { message = "Protocol does not exist or is inactive" });
            if (!PreparingStepType.Matches(protocol.StepType))
                return BadRequest(new { message = "Protocol is not a preparing protocol" });

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.Id == dto.MaterialId);
            if (material is null || !material.IsActive)
                return BadRequest(new { message = "Material does not exist or is inactive" });
            if (material.MaterialType.FlowId is null)
                return BadRequest(new { message = "Material type has no flow configured" });

            var preparingInvId = InventoryIds.Preparing;

            var steps = await _context.FlowSteps
                .Where(s => s.FlowId == material.MaterialType.FlowId.Value)
                .OrderBy(s => s.OrderIndex)
                .Include(s => s.Location)
                .ToListAsync();

            var toStep = steps.FirstOrDefault(s => s.Location.InventoryId == preparingInvId);
            if (toStep is null)
                return BadRequest(new { message = "This material's flow does not include Preparing; it cannot have a preparing order" });

            var previousStep = steps.LastOrDefault(s => s.OrderIndex < toStep.OrderIndex);
            if (previousStep?.Location.InventoryId is null)
                return BadRequest(new { message = "This material's flow starts in Preparing; it cannot have a preparing order" });

            var fromLocation = await _context.Locations
                .FirstOrDefaultAsync(l => l.InventoryId == previousStep.Location.InventoryId && l.Zone == InventoryZones.Accepted);
            if (fromLocation is null)
                return BadRequest(new { message = "Source inventory has no Accepted zone" });

            // First virtual step from the protocol.
            var protocolLocations = await _context.GetProtocolLocationsAsync(protocol.Id);
            if (protocolLocations.Count == 0)
                return BadRequest(new { message = "Protocol has no steps with virtual locations configured" });

            var firstVLocation = protocolLocations[0];

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _transfer.CreateAsync(
                    fromLocation.Id, firstVLocation.Id, material.Id, dto.Quantity,
                    allowOffFlowDestination: true);

                var order = new PreparingOrder
                {
                    Number = dto.Number.Trim(),
                    OrderDate = dto.OrderDate ?? DateTime.UtcNow,
                    ProtocolId = protocol.Id,
                    Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                    TransferOrderId = transfer.Id
                };
                _context.PreparingOrders.Add(order);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _audit.LogAsync(User, "CreatePreparingOrder",
                    $"Created preparing order '{order.Number}' (Id: {order.Id}, TransferOrder: {transfer.Id})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Ok(await ToResponseDto(order.Id));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                await tx.RollbackAsync();
                return Conflict(new { message = "A preparing order with this number already exists" });
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin, Warehouse, Production")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdatePreparingOrderDto dto)
        {
            var order = await _context.PreparingOrders
                .Include(o => o.TransferOrder)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Preparing order not found" });

            if (!string.IsNullOrWhiteSpace(dto.Number)) order.Number = dto.Number.Trim();
            if (dto.OrderDate.HasValue) order.OrderDate = dto.OrderDate.Value;
            if (dto.Notes != null)
                order.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            if (dto.ProtocolId.HasValue)
            {
                var protocol = await _context.Protocols
                    .Include(p => p.StepType)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProtocolId.Value);
                if (protocol is null || !protocol.IsActive)
                    return BadRequest(new { message = "Protocol does not exist or is inactive" });
                if (!PreparingStepType.Matches(protocol.StepType))
                    return BadRequest(new { message = "Protocol is not a preparing protocol" });
                order.ProtocolId = protocol.Id;
            }

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A preparing order with this number already exists" });
            }

            await _audit.LogAsync(User, "UpdatePreparingOrder",
                $"Updated preparing order (Id: {order.Id}, Number: {order.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [Authorize(Roles = "Admin, Warehouse")]
        [HttpPut("{id}/lines")]
        public async Task<IActionResult> SetLines(int id, SetTransferLinesDto dto)
        {
            var order = await _context.PreparingOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Preparing order not found" });
            if (order.TransferOrderId is null) return BadRequest(new { message = "Order has no transfer order" });

            try
            {
                await _transfer.SetLinesAsync(order.TransferOrderId.Value,
                    dto.Lines.Select(l => (l.UomId, l.Quantity)));
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "SetPreparingOrderLines",
                $"Set {dto.Lines.Count} line(s) on preparing order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [Authorize(Roles = "Admin, Warehouse, Production")]
        [HttpPut("{id}/done")]
        public async Task<IActionResult> MarkDone(int id)
        {
            var order = await _context.PreparingOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Preparing order not found" });

            try { await _transfer.MarkDoneAsync(order.TransferOrderId.Value); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _notifications.NotifyAsync(
                "Warehouse",
                $"Preparing order {order.Number} is Done; execute it",
                NotificationRelatedTypes.PreparingOrderDone,
                relatedId: order.Id);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(User, "MarkPreparingOrderDone",
                $"Marked preparing order '{order.Number}' Done (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [Authorize(Roles = "Admin, Warehouse")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.PreparingOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Preparing order not found" });

            try { await _transfer.CancelAsync(order.TransferOrderId.Value); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "CancelPreparingOrder",
                $"Cancelled preparing order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [Authorize(Roles = "Admin, Production, Warehouse Keeper")]
        [HttpPut("{id}/finish")]
        public async Task<IActionResult> Finish(int id, FinishTransferOrderDto dto)
        {
            var order = await _context.PreparingOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null)
                return NotFound(new { message = "Preparing order not found" });

            var transfer = await _context.TransferOrders
                .Include(t => t.Lines).ThenInclude(l => l.Uom)
                .FirstOrDefaultAsync(t => t.Id == order.TransferOrderId.Value);
            if (transfer is null) return NotFound(new { message = "Transfer order not found" });
            if (transfer.Status != TransferOrderStatuses.Done)
                return BadRequest(new { message = "Only a Done transfer order can be finished" });

            var scanned = dto.ScannedUomIds.Distinct().ToList();
            if (scanned.Count == 0)
                return BadRequest(new { message = "No boxes were scanned" });

            var existingOrigins = new List<(int LineId, UomOrigin Origin)>();
            foreach (var line in transfer.Lines)
            {
                var origin = await _context.GetAsync(line.UomId);
                if (origin is null)
                    return BadRequest(new { message = $"Could not resolve origin for box {line.Uom.Barcode}" });
                existingOrigins.Add((line.Id, origin));
            }
            if (existingOrigins.Count == 0)
                return BadRequest(new { message = "Order has no lines to compare against" });

            var acceptedLineIds = new HashSet<int>();
            var newLineUoms = new List<(int UomId, decimal Quantity)>();

            foreach (var uomId in scanned)
            {
                var existingLine = transfer.Lines.FirstOrDefault(l => l.UomId == uomId);
                if (existingLine is not null) { acceptedLineIds.Add(existingLine.Id); continue; }

                var box = await _context.UOMs.FirstOrDefaultAsync(u => u.Id == uomId);
                if (box is null)
                    return BadRequest(new { message = $"Box {uomId} does not exist" });

                var origin = await _context.GetAsync(uomId);
                if (origin is null)
                    return BadRequest(new { message = $"Could not resolve origin for box {box.Barcode}" });

                var matches = existingOrigins.Any(e =>
                    e.Origin.SupplierId == origin.SupplierId
                    && e.Origin.MemoRWInId == origin.MemoRWInId
                    && e.Origin.MaterialId == origin.MaterialId);

                if (!matches)
                    return BadRequest(new { message = $"Box {box.Barcode} does not match the supplier/memo/material of any line on this order" });

                newLineUoms.Add((uomId, box.Quantity));
            }

            var finalLines = transfer.Lines
                .Where(l => acceptedLineIds.Contains(l.Id))
                .Select(l => (l.UomId, l.Quantity))
                .Concat(newLineUoms)
                .ToList();

            try
            {
                await _transfer.SetLinesAsync(transfer.Id, finalLines);
                await _transfer.FinishAsync(transfer.Id, finalLines.Select(l => l.UomId));
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            await _audit.LogAsync(User, "FinishPreparingOrder",
                $"Finished preparing order '{order.Number}' (Id: {order.Id}, moved {finalLines.Count} box(es))",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(order.Id));
        }

        [HttpGet("{id}/candidates")]
        public async Task<IActionResult> Candidates(int id)
        {
            var order = await _context.PreparingOrders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null || order.TransferOrderId is null) return NotFound(new { message = "Preparing order not found" });
            return Ok(await _transfer.GetCandidatesAsync(order.TransferOrderId.Value));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.PreparingOrders
                .Include(o => o.Protocol)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.FromLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.ToLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Material)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Lines).ThenInclude(l => l.Uom)
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id);

            var totalCount = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new PagedResultDto<PreparingOrderResponseDto>
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
            var exists = await _context.PreparingOrders.AnyAsync(o => o.Id == id);
            if (!exists) return NotFound(new { message = "Preparing order not found" });
            return Ok(await ToResponseDto(id));
        }

        [Authorize(Roles = "Admin, Technical, Manager")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var order = await _context.PreparingOrders.FindAsync(id);
            if (order is null) return NotFound(new { message = "Preparing order not found" });

            order.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _audit.LogAsync(User, dto.IsActive ? "ActivatePreparingOrder" : "DeactivatePreparingOrder",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} preparing order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Preparing order {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.PreparingOrders
                .Include(o => o.TransferOrder)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new { message = "Preparing order not found" });

            if (order.TransferOrder is not null
                && order.TransferOrder.Status is not (TransferOrderStatuses.New or TransferOrderStatuses.Cancelled))
                return Conflict(new { message = "Cannot delete: transfer order has already been Done or Executed" });

            _context.PreparingOrders.Remove(order);
            if (order.TransferOrder is not null) _context.TransferOrders.Remove(order.TransferOrder);

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(order);
                return Conflict(conflictPayload);
            }

            await _audit.LogAsync(User, "DeletePreparingOrder",
                $"Deleted preparing order '{order.Number}' (Id: {order.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Preparing order deleted successfully" });
        }

        // ---------------------------------------------------------------
        // Step completion — one box per call
        // ---------------------------------------------------------------

        [HttpPost("{id}/complete-washing")]
        public async Task<IActionResult> CompleteWashing(int id, StepReportLineDto dto) =>
            await AdvanceBoxAsync(id, LocationIds.VirtualWashing, dto, "Washing", PreparingSteps.Washing);

        [HttpPost("{id}/complete-purification")]
        public async Task<IActionResult> CompletePurification(int id, StepReportLineDto dto) =>
            await AdvanceBoxAsync(id, LocationIds.VirtualPurification, dto, "Purification", PreparingSteps.Purification);

        [HttpPost("{id}/complete-sterilization")]
        public async Task<IActionResult> CompleteSterilization(int id, StepReportLineDto dto) =>
            await AdvanceBoxAsync(id, LocationIds.VirtualSterilization, dto, "Sterilization", PreparingSteps.Sterilization);

        [HttpPost("{id}/complete-drying")]
        public async Task<IActionResult> CompleteDrying(int id, StepReportLineDto dto) =>
            await AdvanceBoxAsync(id, LocationIds.VirtualDrying, dto, "Drying", PreparingSteps.Drying);

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private string CurrentUserId() =>
            User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? "unknown";

        private async Task<IActionResult> AdvanceBoxAsync(
            int preparingOrderId,
            int expectedCurrentLocationId,
            StepReportLineDto dto,
            string stepNameEn,
            PreparingSteps step)
        {
            var order = await _context.PreparingOrders
                .FirstOrDefaultAsync(o => o.Id == preparingOrderId);
            if (order is null) return NotFound(new { message = "Preparing order not found" });
            if (order.CompletedAt.HasValue) return BadRequest(new { message = "Preparing order is already completed" });

            var box = await _context.UOMs.FirstOrDefaultAsync(u => u.Id == dto.UomId);
            if (box is null) return NotFound(new { message = "Box not found" });
            if (box.LocationId != expectedCurrentLocationId)
                return Conflict(new { message = $"Box is not in the {stepNameEn} step" });
            if (dto.BeforeWeight != box.Quantity)
                return BadRequest(new { message = $"Before weight {dto.BeforeWeight} does not match box quantity {box.Quantity}" });
            if (dto.AfterWeight > dto.BeforeWeight)
                return BadRequest(new { message = "After weight cannot exceed before weight" });

            var protocolLocations = await _context.GetProtocolLocationsAsync(order.ProtocolId);
            if (protocolLocations.Count == 0)
                return BadRequest(new { message = "Protocol has no steps configured" });

            int nextLocationId;
            bool isLastStep;
            try
            {
                var idx = PreparingStepResolver.GetCurrentStepIndex(protocolLocations, expectedCurrentLocationId);
                if (idx < 0) return BadRequest(new { message = "Current step is not part of this protocol" });
                isLastStep = idx == protocolLocations.Count - 1;
                nextLocationId = await _context.GetNextDestinationAsync(protocolLocations, expectedCurrentLocationId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var userId = CurrentUserId();

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Loss split (may reduce box.Quantity).
                await _context.ApplyLossAsync(box, dto.AfterWeight, $"{stepNameEn} loss", userId);
                await _context.SaveChangesAsync();

                // 2) Move the box.
                var fromLocationId = box.LocationId;
                var fromShelfId = box.ShelfId;
                box.LocationId = nextLocationId;
                box.ShelfId = null;

                var move = new StockMove
                {
                    UomId = box.Id,
                    FromLocationId = fromLocationId,
                    ToLocationId = nextLocationId,
                    FromShelfId = fromShelfId,
                    ToShelfId = null,
                    Quantity = box.Quantity,
                    MoveType = StockMoveTypes.Manual,
                    SourceType = StockMoveSources.PreparingStep,
                    SourceId = order.Id,
                    Reason = $"{stepNameEn} completed",
                    MoveDate = DateTime.UtcNow,
                    UserId = userId
                };
                _context.StockMoves.Add(move);
                await _context.SaveChangesAsync();

                // 3) Report row — one table per step.
                switch (step)
                {
                    case PreparingSteps.Washing:
                        _context.WashingReports.Add(new WashingReport
                        {
                            PreparingOrderId = order.Id,
                            UomId = box.Id,
                            StockMoveId = move.Id,
                            BeforeWeight = dto.BeforeWeight,
                            AfterWeight = dto.AfterWeight,
                            LossPercentage = dto.LossPercentage,
                            Notes = dto.Notes,
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow
                        });
                        break;
                    case PreparingSteps.Purification:
                        _context.PurificationReports.Add(new PurificationReport
                        {
                            PreparingOrderId = order.Id,
                            UomId = box.Id,
                            StockMoveId = move.Id,
                            BeforeWeight = dto.BeforeWeight,
                            AfterWeight = dto.AfterWeight,
                            LossPercentage = dto.LossPercentage,
                            Notes = dto.Notes,
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow
                        });
                        break;
                    case PreparingSteps.Sterilization:
                        _context.SterilizationReports.Add(new SterilizationReport
                        {
                            PreparingOrderId = order.Id,
                            UomId = box.Id,
                            StockMoveId = move.Id,
                            BeforeWeight = dto.BeforeWeight,
                            AfterWeight = dto.AfterWeight,
                            LossPercentage = dto.LossPercentage,
                            Notes = dto.Notes,
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow
                        });
                        break;
                    case PreparingSteps.Drying:
                        _context.DryingReports.Add(new DryingReport
                        {
                            PreparingOrderId = order.Id,
                            UomId = box.Id,
                            StockMoveId = move.Id,
                            BeforeWeight = dto.BeforeWeight,
                            AfterWeight = dto.AfterWeight,
                            LossPercentage = dto.LossPercentage,
                            Notes = dto.Notes,
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow
                        });
                        break;
                }

                // 4) Completion check.
                await TryCompleteOrderAsync(order);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _audit.LogAsync(User, $"Complete{stepNameEn}Step",
                    $"Completed {stepNameEn} for box '{box.Barcode}' in preparing order '{order.Number}' (OrderId: {order.Id}, UomId: {box.Id})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                if (isLastStep)
                {
                    await _notifications.NotifyAsync(
                        "Warehouse",
                        $"Box {box.Barcode} moved to I2 Accepted; put it away",
                        NotificationRelatedTypes.BoxNeedsPutAway,
                        relatedId: box.Id);
                    await _context.SaveChangesAsync();
                }

                return Ok(await ToResponseDto(order.Id));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task TryCompleteOrderAsync(PreparingOrder order)
        {
            if (order.TransferOrderId is null) return;

            var firstTransfer = await _context.TransferOrders
                .Include(t => t.Lines)
                .FirstOrDefaultAsync(t => t.Id == order.TransferOrderId.Value);
            if (firstTransfer is null) return;

            var initialBoxIds = firstTransfer.Lines.Select(l => l.UomId).ToList();
            if (initialBoxIds.Count == 0) return;

            var notDoneCount = await _context.UOMs
                .Where(u => initialBoxIds.Contains(u.Id)
                            && u.LocationId != LocationIds.Scrap)
                .Join(_context.Locations, u => u.LocationId, l => l.Id, (u, l) => l)
                .CountAsync(l => !(l.InventoryId == InventoryIds.Preparing && l.Zone == InventoryZones.Accepted));

            if (notDoneCount == 0 && !order.CompletedAt.HasValue)
                order.CompletedAt = DateTime.UtcNow;
        }

        private async Task<PreparingOrderResponseDto> ToResponseDto(int id)
        {
            var order = await _context.PreparingOrders
                .Include(o => o.Protocol)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.FromLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.ToLocation)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Material)
                .Include(o => o.TransferOrder!).ThenInclude(t => t.Lines).ThenInclude(l => l.Uom).ThenInclude(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .FirstAsync(o => o.Id == id);

            var dto = ToDto(order);

            if (dto.TransferOrder is not null)
            {
                foreach (var line in dto.TransferOrder.Lines)
                {
                    var origin = await _context.GetAsync(line.UomId);
                    if (origin is null) continue;
                    line.SupplierId = origin.SupplierId;
                    line.SupplierNumber = origin.SupplierNumber;
                    line.SupplierNameEn = origin.SupplierNameEn;
                    line.SupplierNameAr = origin.SupplierNameAr;
                    line.MemoRWInId = origin.MemoRWInId;
                    line.MemoRWInNumber = origin.MemoRWInNumber;
                }
            }

            return dto;
        }

        private static PreparingOrderResponseDto ToDto(PreparingOrder o)
        {
            return new PreparingOrderResponseDto
            {
                Id = o.Id,
                Number = o.Number,
                OrderDate = o.OrderDate,
                ProtocolId = o.ProtocolId,
                ProtocolNumber = o.Protocol?.ProtocolNumber ?? string.Empty,
                ProtocolNameEn = o.Protocol?.NameEn ?? string.Empty,
                ProtocolNameAr = o.Protocol?.NameAr ?? string.Empty,
                Notes = o.Notes,
                IsActive = o.IsActive,
                IsCompleted = o.CompletedAt.HasValue,
                CompletedAt = o.CompletedAt,
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