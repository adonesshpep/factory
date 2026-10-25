// DTOs/MemoResponseDto.cs
using neotonic.Models;

namespace neotonic.DTOs
{
    public class MemoResponseDto
    {
        public int Id { get; set; }
        public long Number { get; set; }
        public DateTime MemoDate { get; set; }
        public DateTime CreationDate { get; set; }

        public MemoType TypeId { get; set; }
        public string TypeNameEn { get; set; } = string.Empty;
        public string TypeNameAr { get; set; } = string.Empty;

        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }

        // Supplier — null for Out memos
        public int? SupplierId { get; set; }
        public string? SupplierNumber { get; set; }
        public string? SupplierNameEn { get; set; }
        public string? SupplierNameAr { get; set; }

        // Work order — null if not linked
        public int? WorkOrderId { get; set; }
        public long? WorkOrderNumber { get; set; }

        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        // ---- Memo-level totals ----
        public int TotalDetailCount { get; set; }
        public int TotalUOMCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalUOMQuantity { get; set; }

        public List<MemoDetailResponseDto> Details { get; set; } = new();
    }
}