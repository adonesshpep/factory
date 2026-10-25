using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class UpdateMemoRWInDto
    {
        public DateTime? MemoDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? PurchaseOrderId { get; set; }

        [Range(1, int.MaxValue)]
        public int? WorkerId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}