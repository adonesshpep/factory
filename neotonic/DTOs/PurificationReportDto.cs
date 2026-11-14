using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class PurificationReportDto
    {
        [Required, Range(1, int.MaxValue)]
        public int UomId { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal BeforeWeight { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal AfterWeight { get; set; }

        [Required, Range(typeof(decimal), "0", "100")]
        public decimal LossPercentage { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal TheoreticalImpurityWeight { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal PracticalImpurityWeight { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int WorkerId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}