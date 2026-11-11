using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateSupplierReturnOrderDto
    {
        [Required, MaxLength(50)] public string Number { get; set; } = string.Empty;
        public DateTime? OrderDate { get; set; }

        [Required, Range(1, int.MaxValue)] public int SupplierId { get; set; }
        [Required, Range(1, int.MaxValue)] public int MaterialId { get; set; }
        [Required, Range(1, int.MaxValue)] public int FromLocationId { get; set; }
        [Required, Range(typeof(decimal), "0", "999999999999999")] public decimal Quantity { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }
    }
}