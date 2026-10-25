namespace neotonic.DTOs.Responses
{
    public class ProtocolResponseDto
    {
        public int Id { get; set; }
        public string ProtocolNumber { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int StepTypeId { get; set; }
        public string StepTypeNameEn { get; set; } = string.Empty;
        public string StepTypeNameAr { get; set; } = string.Empty;

        public List<ProtocolStepResponseDto> Steps { get; set; } = new();
    }
}
