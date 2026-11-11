// Models/CertificateOfAnalysis.cs  (CHANGED)
// SampleId replaces SupplierSampleId. Result is NOT filled at create/update:
// it is only set through PUT certificatesofanalysis/result/{id} (admin policy).
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("certificates_of_analysis")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_certificates_of_analysis_number")]
    public class CertificateOfAnalysis
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("analysis_date")]
        public DateTime AnalysisDate { get; set; }

        public int SampleId { get; set; }

        [ForeignKey(nameof(SampleId))]
        public Sample Sample { get; set; } = null!;

        [MaxLength(200)]
        [Column("physical_appearance")]
        public string? PhysicalAppearance { get; set; }

        [Column("moisture_percentage", TypeName = "decimal(5,2)")]
        public decimal? MoisturePercentage { get; set; }

        [MaxLength(500)]
        [Column("bacterial_test")]
        public string? BacterialTest { get; set; }

        [MaxLength(500)]
        [Column("fungal_test")]
        public string? FungalTest { get; set; }

        [MaxLength(500)]
        [Column("microscopic_appearance")]
        public string? MicroscopicAppearance { get; set; }

        [MaxLength(200)]
        [Column("color")]
        public string? Color { get; set; }

        [MaxLength(200)]
        [Column("odor")]
        public string? Odor { get; set; }

        // Pending until the admin result route sets it.
        [Column("result")]
        public AnalysisResults Result { get; set; } = AnalysisResults.Pending;

        [MaxLength(1000)]
        [Column("rejection_reason")]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}