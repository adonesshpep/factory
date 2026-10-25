// DTOs/UpdateMemoInDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class UpdateMemoInDto
    {
        public long? Number { get; set; }

        public DateTime? MemoDate { get; set; }

        [MaxLength(50)]
        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? SupplierId { get; set; }

        public int? WorkOrderId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // null = leave details unchanged. Provided (even empty) = full replace.
        public List<UpdateMemoDetailDto>? Details { get; set; }
    }
}