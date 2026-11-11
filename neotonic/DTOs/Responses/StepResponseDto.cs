namespace neotonic.DTOs.Responses
{
    public class StepResponseDto
    {
        public int Id { get; set; }
        public int StepNumber { get; set; }
        public string StepNameEn { get; set; } = string.Empty;
        public string StepNameAr { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? BoxPathId { get; set; }
        public string? BoxPathName { get; set; }
        public string? BoxPathCode { get; set; }
        public int StepTypeId { get; set; }
        public string StepTypeNameEn { get; set; } = string.Empty;
        public string StepTypeNameAr { get; set; } = string.Empty;
        public int? VirtualLocationId { get; set; }
        public string? VirtualLocationCode { get; set; }
        public string? VirtualLocationNameEn { get; set; }
        public string? VirtualLocationNameAr { get; set; }
        public string Color { get; set; } = "#FFFFFF";
        public bool IsActive { get; set; }
    }
}