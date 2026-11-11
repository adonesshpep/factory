using Microsoft.EntityFrameworkCore;
using neotonic.DTOs.Responses;
using neotonic.Models;

namespace neotonic.Services
{
    public class TransferOrderService : ITransferOrderService
    {
        private readonly AppDbContext _context;

        public TransferOrderService(AppDbContext context) => _context = context;

        public async Task<decimal> GetReservedAsync(int uomId, CancellationToken ct = default)
        {
            return await _context.TransferOrderLines
                .Where(l => l.UomId == uomId && l.TransferOrder.Status == TransferOrderStatuses.Done)
                .SumAsync(l => (decimal?)l.Quantity, ct) ?? 0m;
        }

        public async Task<decimal> GetAvailableAsync(int uomId, CancellationToken ct = default)
        {
            var boxQuantity = await _context.UOMs.AsNoTracking()
                .Where(u => u.Id == uomId)
                .Select(u => (decimal?)u.Quantity)
                .FirstOrDefaultAsync(ct);
            if (boxQuantity is null) return 0m;
            return boxQuantity.Value - await GetReservedAsync(uomId, ct);
        }

        public async Task<TransferOrder> CreateAsync(int fromLocationId, int toLocationId,
            int materialId, decimal quantity, bool allowOffFlowDestination = false,
            CancellationToken ct = default)
        {
            if (fromLocationId == toLocationId)
                throw new InvalidOperationException("From and To locations must differ");
            if (quantity < 0)
                throw new InvalidOperationException("Quantity must be >= 0");

            var from = await _context.Locations.FirstOrDefaultAsync(l => l.Id == fromLocationId && l.IsActive, ct)
                       ?? throw new InvalidOperationException("From location does not exist or is inactive");
            var to = await _context.Locations.FirstOrDefaultAsync(l => l.Id == toLocationId && l.IsActive, ct)
                     ?? throw new InvalidOperationException("To location does not exist or is inactive");
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == materialId && m.IsActive, ct)
                           ?? throw new InvalidOperationException("Material does not exist or is inactive");

            var flowId = await _context.MaterialTypes
                .Where(t => t.Id == material.MaterialTypeId)
                .Select(t => t.FlowId)
                .FirstAsync(ct);

            if (flowId is not null)
            {
                var steps = await _context.FlowSteps
                    .Where(s => s.FlowId == flowId)
                    .Select(s => new { s.OrderIndex, s.LocationId })
                    .ToListAsync(ct);

                var fromStep = steps.FirstOrDefault(s => s.LocationId == fromLocationId);
                var toStep = steps.FirstOrDefault(s => s.LocationId == toLocationId);

                if (!allowOffFlowDestination)
                {
                    if (fromStep is not null && toStep is not null && fromStep.OrderIndex >= toStep.OrderIndex)
                        throw new InvalidOperationException("Destination must be a later step in the material's flow");
                    if (fromStep is not null && toStep is null)
                        throw new InvalidOperationException("Destination is not on the material's flow");
                }
            }

            var order = new TransferOrder
            {
                FromLocationId = from.Id,
                ToLocationId = to.Id,
                MaterialId = material.Id,
                Quantity = quantity,
                Status = TransferOrderStatuses.New
            };
            _context.TransferOrders.Add(order);
            await _context.SaveChangesAsync(ct);
            return order;
        }

        public async Task<TransferOrder> SetLinesAsync(int transferOrderId,
            IEnumerable<(int UomId, decimal Quantity)> lines, CancellationToken ct = default)
        {
            var order = await _context.TransferOrders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == transferOrderId, ct)
                ?? throw new InvalidOperationException("Transfer order not found");

            //if (order.Status != TransferOrderStatuses.New)
            //    throw new InvalidOperationException("Lines can only be changed while the transfer order is New");

            var incoming = lines.ToList();
            if (incoming.Select(l => l.UomId).Distinct().Count() != incoming.Count)
                throw new InvalidOperationException("The same box cannot appear twice on one order");

            foreach (var l in incoming)
            {
                if (l.Quantity <= 0)
                    throw new InvalidOperationException("Line quantity must be greater than zero");

                var box = await _context.UOMs.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == l.UomId, ct)
                    ?? throw new InvalidOperationException($"Box {l.UomId} does not exist");

                if (box.MaterialId != order.MaterialId || box.Kind != UOMKinds.Material)
                    throw new InvalidOperationException($"Box {box.Barcode} is not a material box of this order's material");
                if (box.LocationId != order.FromLocationId)
                    throw new InvalidOperationException($"Box {box.Barcode} is not in the source location");
                if (l.Quantity > box.Quantity)
                    throw new InvalidOperationException($"Line quantity exceeds box quantity for {box.Barcode}");
            }

            _context.TransferOrderLines.RemoveRange(order.Lines);
            foreach (var l in incoming)
                _context.TransferOrderLines.Add(new TransferOrderLine
                {
                    TransferOrderId = order.Id,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    IsScanned = false
                });

            await _context.SaveChangesAsync(ct);
            return order;
        }

        public async Task<TransferOrder> MarkDoneAsync(int transferOrderId, CancellationToken ct = default)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var order = await _context.TransferOrders
                    .Include(o => o.Lines)
                    .FirstOrDefaultAsync(o => o.Id == transferOrderId, ct)
                    ?? throw new InvalidOperationException("Transfer order not found");

                //if (order.Status != TransferOrderStatuses.New)
                //    throw new InvalidOperationException("Only a New transfer order can be marked Done");

                if (order.Lines.Count == 0)
                    throw new InvalidOperationException("Cannot mark Done an order with no lines");

                var uomIds = order.Lines.Select(l => l.UomId).ToList();

                await _context.Database.ExecuteSqlRawAsync(
                    "SELECT 1 FROM uoms WITH (UPDLOCK, HOLDLOCK) WHERE Id IN (SELECT UomId FROM transfer_order_lines WHERE TransferOrderId = {0})",
                    new object[] { transferOrderId }, ct);

                var reservedElsewhere = await _context.TransferOrderLines
                    .Where(l => uomIds.Contains(l.UomId)
                                && l.TransferOrderId != order.Id
                                && l.TransferOrder.Status == TransferOrderStatuses.Done)
                    .GroupBy(l => l.UomId)
                    .Select(g => new { UomId = g.Key, Qty = g.Sum(x => x.Quantity) })
                    .ToDictionaryAsync(x => x.UomId, x => x.Qty, ct);

                var boxes = await _context.UOMs.AsNoTracking()
                    .Where(u => uomIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u, ct);

                foreach (var line in order.Lines)
                {
                    if (!boxes.TryGetValue(line.UomId, out var box))
                        throw new InvalidOperationException($"Box {line.UomId} no longer exists");

                    var reserved = reservedElsewhere.GetValueOrDefault(line.UomId, 0m);
                    var available = box.Quantity - reserved;
                    if (line.Quantity > available)
                        throw new InvalidOperationException(
                            $"Box {box.Barcode} has only {available} available (line wants {line.Quantity})");
                }

                order.Status = TransferOrderStatuses.Done;
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return order;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<TransferOrder> CancelAsync(int transferOrderId, CancellationToken ct = default)
        {
            var order = await _context.TransferOrders
                .FirstOrDefaultAsync(o => o.Id == transferOrderId, ct)
                ?? throw new InvalidOperationException("Transfer order not found");

            if (order.Status is not (TransferOrderStatuses.New or TransferOrderStatuses.Done))
                throw new InvalidOperationException("Only New or Done transfer orders can be cancelled");

            order.Status = TransferOrderStatuses.Cancelled;
            await _context.SaveChangesAsync(ct);
            return order;
        }

        public async Task<TransferOrder> FinishAsync(int transferOrderId, IEnumerable<int> scannedUomIds,
            CancellationToken ct = default)
        {
            var scanned = scannedUomIds.Distinct().ToList();

            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var order = await _context.TransferOrders
                    .Include(o => o.Lines)
                    .FirstOrDefaultAsync(o => o.Id == transferOrderId, ct)
                    ?? throw new InvalidOperationException("Transfer order not found");

                if (order.Status != TransferOrderStatuses.Done)
                    throw new InvalidOperationException("Only a Done transfer order can be finished");

                await _context.Database.ExecuteSqlRawAsync(
                    "SELECT 1 FROM uoms WITH (UPDLOCK, HOLDLOCK) WHERE Id IN (SELECT UomId FROM transfer_order_lines WHERE TransferOrderId = {0})",
                    new object[] { transferOrderId }, ct);

                var linesByUom = order.Lines.ToDictionary(l => l.UomId);
                var boxes = await _context.UOMs
                    .Where(u => linesByUom.Keys.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u, ct);

                foreach (var uomId in scanned)
                {
                    if (!linesByUom.TryGetValue(uomId, out var line))
                        throw new InvalidOperationException($"Box {uomId} is not on this transfer order");
                    if (!boxes.TryGetValue(uomId, out var box))
                        throw new InvalidOperationException($"Box {uomId} no longer exists");

                    if (line.Quantity == box.Quantity)
                    {
                        var fromLocation = box.LocationId;
                        var fromShelf = box.ShelfId;
                        box.LocationId = order.ToLocationId;
                        box.ShelfId = null;

                        _context.StockMoves.Add(new StockMove
                        {
                            UomId = box.Id,
                            FromLocationId = fromLocation,
                            ToLocationId = order.ToLocationId,
                            FromShelfId = fromShelf,
                            ToShelfId = null,
                            Quantity = box.Quantity,
                            MoveType = StockMoveTypes.Move,
                            SourceType = StockMoveSources.TransferOrder,
                            SourceId = order.Id,
                            Reason = null,
                            MoveDate = DateTime.UtcNow,
                            UserId = "system"
                        });
                    }
                    else if (line.Quantity < box.Quantity)
                    {
                        var nextNumber = (await _context.UOMs.Select(u => (long?)u.Number).MaxAsync(ct) ?? 0L) + 1;
                        var child = new UOM
                        {
                            Barcode = nextNumber.ToString("D8"),
                            Number = nextNumber,
                            Kind = box.Kind,
                            MaterialId = box.MaterialId,
                            ProductId = box.ProductId,
                            Lot = box.Lot,
                            Quantity = line.Quantity,
                            LocationId = order.ToLocationId,
                            ShelfId = null,
                            ParentUomId = box.Id
                        };
                        _context.UOMs.Add(child);
                        await _context.SaveChangesAsync(ct);

                        box.Quantity -= line.Quantity;

                        _context.StockMoves.Add(new StockMove
                        {
                            UomId = child.Id,
                            FromLocationId = box.LocationId,
                            ToLocationId = order.ToLocationId,
                            FromShelfId = box.ShelfId,
                            ToShelfId = null,
                            Quantity = child.Quantity,
                            MoveType = StockMoveTypes.Split,
                            SourceType = StockMoveSources.TransferOrder,
                            SourceId = order.Id,
                            Reason = null,
                            MoveDate = DateTime.UtcNow,
                            UserId = "system"
                        });
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Line quantity exceeds box quantity for {box.Barcode}");
                    }

                    line.IsScanned = true;
                }

                var unscanned = order.Lines.Where(l => !l.IsScanned).ToList();
                _context.TransferOrderLines.RemoveRange(unscanned);

                order.Status = TransferOrderStatuses.Executed;
                order.ExecutedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return order;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<List<UomListItemDto>> GetCandidatesAsync(int transferOrderId, CancellationToken ct = default)
        {
            var order = await _context.TransferOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == transferOrderId, ct)
                ?? throw new InvalidOperationException("Transfer order not found");

            var boxes = await _context.UOMs.AsNoTracking()
                .Where(u => u.LocationId == order.FromLocationId
                         && u.Kind == UOMKinds.Material
                         && u.MaterialId == order.MaterialId)
                .Include(u => u.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Shelf).ThenInclude(s => s!.Location).ThenInclude(l => l.Inventory)
                .Include(u => u.Material)
                .ToListAsync(ct);

            if (boxes.Count == 0) return new List<UomListItemDto>();

            var ids = boxes.Select(b => b.Id).ToList();

            var reserved = await _context.TransferOrderLines
                .Where(l => ids.Contains(l.UomId) && l.TransferOrder.Status == TransferOrderStatuses.Done)
                .GroupBy(l => l.UomId)
                .Select(g => new { UomId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.UomId, x => x.Qty, ct);

            // Resolve memo/PO/supplier chain for these boxes.
            var originPerBox = await _context.MemoRWDetails
                .Where(d => ids.Contains(d.UomId))
                .Select(d => new
                {
                    d.UomId,
                    MemoRWInId = d.Memo.Id,
                    MemoRWInNumber = d.Memo.Number,
                    d.Memo.PurchaseOrderId,
                    d.Memo.PurchaseOrder.Number,
                    SupplierId = d.Memo.PurchaseOrder.ContractReview.SupplierId,
                    d.Memo.PurchaseOrder.ContractReview.Supplier.SupplierNumber,
                    d.Memo.PurchaseOrder.ContractReview.Supplier.NameEn,
                    d.Memo.PurchaseOrder.ContractReview.Supplier.NameAr
                })
                .ToDictionaryAsync(x => x.UomId, x => x, ct);

            var result = new List<UomListItemDto>();
            foreach (var b in boxes)
            {
                var available = b.Quantity - reserved.GetValueOrDefault(b.Id, 0m);
                if (available <= 0) continue;

                originPerBox.TryGetValue(b.Id, out var origin);

                result.Add(new UomListItemDto
                {
                    Id = b.Id,
                    Barcode = b.Barcode,
                    Kind = b.Kind,
                    MaterialId = b.MaterialId,
                    MaterialNumber = b.Material?.MaterialNumber,
                    MaterialNameEn = b.Material?.NameEn,
                    MaterialNameAr = b.Material?.NameAr,
                    Quantity = available,
                    LocationId = b.LocationId,
                    LocationCode = b.Location.Code,
                    InventoryId = b.Location.InventoryId,
                    InventoryCode = b.Location.Inventory?.Code,
                    Zone = b.Location.Zone,
                    ShelfId = b.ShelfId,
                    ShelfLabel = ShelfLabel(b.Shelf),

                    MemoRWInId = origin?.MemoRWInId,
                    MemoRWInNumber = origin?.MemoRWInNumber,
                    SupplierId = origin?.SupplierId,
                    SupplierNumber = origin?.SupplierNumber,
                    SupplierNameEn = origin?.NameEn,
                    SupplierNameAr = origin?.NameAr
                });
            }
            return result;
        }
        private static string? ShelfLabel(Shelf? shelf)
        {
            if (shelf is null) return null;
            var zoneLetter = shelf.Location.Zone switch
            {
                InventoryZones.Quarantine => "A",
                InventoryZones.Accepted => "B",
                InventoryZones.Rejected => "C",
                _ => "?"
            };
            return $"{shelf.Location.Inventory!.Code}-{zoneLetter}{shelf.Number}";
        }
    }
}