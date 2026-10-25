namespace neotonic.DTOs.Responses
{
    public class ScaleEqualizationResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime EqualizationDate { get; set; }

        public int ScaleId { get; set; }
        public string ScaleNumber { get; set; } = string.Empty;
        public string ScaleNameEn { get; set; } = string.Empty;
        public string ScaleNameAr { get; set; } = string.Empty;

        public int WorkerId { get; set; }
        public int WorkerNumber { get; set; }
        public string WorkerNameEn { get; set; } = string.Empty;
        public string WorkerNameAr { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}