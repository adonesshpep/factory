using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class StepReportLineDto
    {
        [Required, Range(1, int.MaxValue)] public int UomId { get; set; }
        [Required, Range(typeof(decimal), "0", "999999999999999")] public decimal BeforeWeight { get; set; }
        [Required, Range(typeof(decimal), "0", "999999999999999")] public decimal AfterWeight { get; set; }
        [Range(typeof(decimal), "0", "100")] public decimal? LossPercentage { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }
}