// DTOs/UpdateMemoOutDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class UpdateMemoOutDto
    {
        public long? Number { get; set; }

        public DateTime? MemoDate { get; set; }

        [MaxLength(50)]
        public string? InvoiceNumber { get; set; }

        public int? WorkOrderId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public List<UpdateMemoOutDetailDto>? Details { get; set; }
    }
}