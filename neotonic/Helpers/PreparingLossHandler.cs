using Microsoft.EntityFrameworkCore;
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class PreparingLossHandler
    {
        // Applies the loss by splitting off a Scrap child. Parent keeps the reduced quantity.
        // ACTIVE STRATEGY. The alternative (direct quantity modification) is commented out below.
        public static async Task ApplyLossAsync(
            this AppDbContext context,
            UOM box,
            decimal afterWeight,
            string reason,
            string userId,
            CancellationToken ct = default)
        {
            if (afterWeight < 0)
                throw new InvalidOperationException("After weight cannot be negative");
            if (afterWeight > box.Quantity)
                throw new InvalidOperationException("After weight cannot exceed the current quantity");
            if (afterWeight == box.Quantity)
                return;

            var lossQuantity = box.Quantity - afterWeight;

            var nextNumber = (await context.UOMs.Select(u => (long?)u.Number).MaxAsync(ct) ?? 0L) + 1;

            var scrapChild = new UOM
            {
                Barcode = nextNumber.ToString("D8"),
                Number = nextNumber,
                Kind = box.Kind,
                MaterialId = box.MaterialId,
                ProductId = box.ProductId,
                Lot = box.Lot,
                Quantity = lossQuantity,
                LocationId = LocationIds.Scrap,
                ShelfId = null,
                ParentUomId = box.Id
            };
            context.UOMs.Add(scrapChild);
            await context.SaveChangesAsync(ct);

            var fromLocationId = box.LocationId;
            var fromShelfId = box.ShelfId;
            box.Quantity = afterWeight;

            context.StockMoves.Add(new StockMove
            {
                UomId = scrapChild.Id,
                FromLocationId = fromLocationId,
                ToLocationId = LocationIds.Scrap,
                FromShelfId = fromShelfId,
                ToShelfId = null,
                Quantity = lossQuantity,
                MoveType = StockMoveTypes.Split,
                SourceType = StockMoveSources.None,
                SourceId = null,
                Reason = reason,
                MoveDate = DateTime.UtcNow,
                UserId = userId
            });
            await context.SaveChangesAsync(ct);

            // ---------- ALTERNATIVE (commented out): direct quantity modification ----------
            // Uncomment and remove the block above if PM picks this strategy later.
            //
            // box.Quantity = afterWeight;
            // context.StockMoves.Add(new StockMove
            // {
            //     UomId = box.Id,
            //     FromLocationId = box.LocationId,
            //     ToLocationId = box.LocationId,
            //     FromShelfId = box.ShelfId,
            //     ToShelfId = box.ShelfId,
            //     Quantity = lossQuantity,
            //     MoveType = StockMoveTypes.Manual,
            //     SourceType = StockMoveSources.None,
            //     SourceId = null,
            //     Reason = reason,
            //     MoveDate = DateTime.UtcNow,
            //     UserId = userId
            // });
            // await context.SaveChangesAsync(ct);
        }
    }
}