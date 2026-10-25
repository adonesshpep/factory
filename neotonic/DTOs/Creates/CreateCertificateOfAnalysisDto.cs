// DTOs/CreateCertificateOfAnalysisDto.cs
using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateCertificateOfAnalysisDto
    {
        [Required, MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public DateTime? AnalysisDate { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int SupplierSampleId { get; set; }
        [MaxLength(200)] public string? PhysicalAppearance { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal? MoisturePercentage { get; set; }

        [MaxLength(500)] public string? BacterialTest { get; set; }
        [MaxLength(500)] public string? FungalTest { get; set; }
        [MaxLength(500)] public string? MicroscopicAppearance { get; set; }
        [MaxLength(200)] public string? Color { get; set; }
        [MaxLength(200)] public string? Odor { get; set; }


        [Required, EnumDataType(typeof(AnalysisResults))]
        public AnalysisResults? Result { get; set; }

        [MaxLength(1000)] public string? RejectionReason { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }
}