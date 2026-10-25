// DTOs/CreateMemoInDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class CreateMemoInDto
    {
        // If <= 0, the controller assigns the next number.
        public long Number { get; set; }

        public DateTime? MemoDate { get; set; }

        [MaxLength(50)]
        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int SupplierId { get; set; }

       

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required, MinLength(1)]
        public List<CreateMemoDetailDto> Details { get; set; } = new();
    }
}