using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class UpdateMemoRWDetailDto
    {
        [Required, Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal Quantity { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal Tear { get; set; }

        public int? ShelfId { get; set; }
    }
}