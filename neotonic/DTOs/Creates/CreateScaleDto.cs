using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateScaleDto
    {
        [Required, MaxLength(50)]
        public string ScaleNumber { get; set; } = string.Empty;

        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int DepartmentId { get; set; }
    }
}
