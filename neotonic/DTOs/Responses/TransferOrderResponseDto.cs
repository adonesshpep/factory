using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class TransferOrderResponseDto
    {
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public string FromLocationCode { get; set; } = string.Empty;
        public int ToLocationId { get; set; }
        public string ToLocationCode { get; set; } = string.Empty;

        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal SatisfiedQuantity { get; set; }
        public TransferOrderStatuses Status { get; set; }
        public string StatusNameEn { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;
        public DateTime? ExecutedAt { get; set; }

        public List<TransferOrderLineResponseDto> Lines { get; set; } = new();
    }

    public class TransferOrderLineResponseDto
    {
        public int Id { get; set; }
        public int UomId { get; set; }
        public string UomBarcode { get; set; } = string.Empty;
        public decimal BoxQuantity { get; set; }
        public decimal LineQuantity { get; set; }
        public bool IsScanned { get; set; }
        public int? ShelfId { get; set; }
        public string? ShelfLabel { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierNumber { get; set; }
        public string? SupplierNameEn { get; set; }
        public string? SupplierNameAr { get; set; }
        public int? MemoRWInId { get; set; }
        public long? MemoRWInNumber { get; set; }
    }
}