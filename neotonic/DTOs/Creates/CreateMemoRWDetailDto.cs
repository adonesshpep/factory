using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class CreateMemoRWDetailDto
    {
        [Required, Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [Required, Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal Quantity { get; set; }

        [Required, MaxLength(50)]
        public string Barcode { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "999999999999999")]
        public decimal Tear { get; set; }

        public int? ShelfId { get; set; }
    }
}