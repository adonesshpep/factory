using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class CreateMemoRWInDto
    {
        public DateTime? MemoDate { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int PurchaseOrderId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int WorkerId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}