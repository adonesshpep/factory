using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class SterilizationReportDto
    {
        [Required, Range(1, int.MaxValue)]
        public int UomId { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal BeforeWeight { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal AfterWeight { get; set; }

        [Required, Range(typeof(decimal), "0", "100")]
        public decimal LossPercentage { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Duration { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal Wavelength { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal Thickness { get; set; }

        [Required, Range(typeof(decimal), "0", "999999999999999")]
        public decimal Speed { get; set; }

        [Required, Range(typeof(decimal), "-100", "100")]
        public decimal Temperature { get; set; }

        [Required, Range(typeof(decimal), "0", "100")]
        public decimal HumidityPercentage { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int SampleId { get; set; }


        [Required, Range(1, int.MaxValue)]
        public int WorkerId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}