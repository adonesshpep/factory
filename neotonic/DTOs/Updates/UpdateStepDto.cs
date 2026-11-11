using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateStepDto
    {
        public int? StepNumber { get; set; }
        public string? StepNameEn { get; set; }
        public string? StepNameAr { get; set; }
        public int? VirtualLocationId { get; set; }
        public string? Notes { get; set; }
        public int? BoxPathId { get; set; }
        public string? Color { get; set; }
        [Range(1, int.MaxValue)] public int? StepTypeId { get; set; }
    }
}
