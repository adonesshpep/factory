using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateScaleEqualizationDto
    {
        [MaxLength(50)] public string? Number { get; set; }
        public DateTime? EqualizationDate { get; set; }

        [Range(1, int.MaxValue)] public int? ScaleId { get; set; }
        [Range(1, int.MaxValue)] public int? WorkerId { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }
    }
}