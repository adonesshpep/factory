using neotonic.DTOs.Responses;
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class TransferDtoMapper
    {
        public static TransferOrderResponseDto ToDto(TransferOrder t)
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
                    ShelfLabel = ShelfLabel(l.Uom?.Shelf)
                }).ToList()
            };
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