using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class CreateMemoRWDetailDto
    {
        [Required]
        public long? Barcode { get; set; }

        [Required]
        public decimal? AWeight { get; set; }

        [Required]
        public decimal? BWeight { get; set; }
    }
}