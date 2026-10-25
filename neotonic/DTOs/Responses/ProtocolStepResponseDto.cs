namespace neotonic.DTOs.Responses
{
    public class ProtocolStepResponseDto
    {
        public int OrderIndex { get; set; }
        public int StepId { get; set; }
        public int StepNumber { get; set; }
        public string StepNameEn { get; set; } = string.Empty;
        public string StepNameAr { get; set; } = string.Empty;
    }
}
