namespace neotonic.DTOs.Responses
{
    public class SterilizationReportResponseDto
    {
        public int Id { get; set; }
        public int PreparingOrderId { get; set; }
        public string PreparingOrderNumber { get; set; } = string.Empty;

        public int UomId { get; set; }
        public string UomBarcode { get; set; } = string.Empty;

        public int StockMoveId { get; set; }

        public decimal BeforeWeight { get; set; }
        public decimal AfterWeight { get; set; }
        public decimal? LossPercentage { get; set; }

        public int Duration { get; set; }
        public decimal Wavelength { get; set; }
        public decimal Thickness { get; set; }
        public decimal Speed { get; set; }
        public decimal Temperature { get; set; }
        public decimal HumidityPercentage { get; set; }

        public int SampleId { get; set; }
        public string SampleNumber { get; set; } = string.Empty;

        public int WorkerId { get; set; }
        public int WorkerNumber { get; set; }
        public string WorkerNameEn { get; set; } = string.Empty;
        public string WorkerNameAr { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}