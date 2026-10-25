using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateScaleEqualizationDto
    {
        [Required, MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public DateTime? EqualizationDate { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ScaleId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int WorkerId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}