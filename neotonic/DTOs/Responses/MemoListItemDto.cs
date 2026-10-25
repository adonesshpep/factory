// DTOs/MemoListItemDto.cs
using neotonic.Models;

namespace neotonic.DTOs
{
    public class MemoListItemDto
    {
        public int Id { get; set; }
        public long Number { get; set; }
        public DateTime MemoDate { get; set; }
        public DateTime CreationDate { get; set; }

        public MemoType TypeId { get; set; }
        public string TypeNameEn { get; set; } = string.Empty;
        public string TypeNameAr { get; set; } = string.Empty;

        public string? InvoiceNumber { get; set; }

        public int? SupplierId { get; set; }
        public string? SupplierNameEn { get; set; }
        public string? SupplierNameAr { get; set; }

        public int? WorkOrderId { get; set; }
        public long? WorkOrderNumber { get; set; }

        public bool IsActive { get; set; }
        public int DetailCount { get; set; }
    }
}