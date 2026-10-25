// DTOs/CreateMemoOutDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class CreateMemoOutDto
    {
        public long Number { get; set; }

        public DateTime? MemoDate { get; set; }

        [MaxLength(50)]
        public string? InvoiceNumber { get; set; }

        public int? WorkOrderId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required, MinLength(1)]
        public List<CreateMemoOutDetailDto> Details { get; set; } = new();
    }
}