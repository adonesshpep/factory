using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateStepDto
    {
        [Required] public int? StepNumber { get; set; }
        public string? StepNameEn { get; set; }
        public string? StepNameAr { get; set; }
        public int? VirtualLocationId { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public int? BoxPathId { get; set; }
        [MaxLength(500)] public string? Color { get; set; } = "#FFFFFF";
        [Required, Range(1, int.MaxValue)] public int StepTypeId { get; set; }
    }
}
