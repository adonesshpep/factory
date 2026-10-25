using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class CertificateOfAnalysisResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }

        // Sample
        public int SupplierSampleId { get; set; }
        public string SupplierSampleNumber { get; set; } = string.Empty;
        public DateTime SampleDeliveryDate { get; set; }

        // Supplier (from the sample)
        public int SupplierId { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;
        public string SupplierNameEn { get; set; } = string.Empty;
        public string SupplierNameAr { get; set; } = string.Empty;

        // Contract review (from the sample)
        public int ContractReviewId { get; set; }
        public string ContractReviewNumber { get; set; } = string.Empty;
        public DateTime ContractReviewCreationDate { get; set; }
        public DateTime ContractReviewExpectedDate { get; set; }
        public decimal ContractReviewQuantity { get; set; }
        public CRStatus ContractReviewStatus { get; set; }
        public string ContractReviewStatusNameEn { get; set; } = string.Empty;
        public string ContractReviewStatusNameAr { get; set; } = string.Empty;

        // Material (from the contract review)
        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        // Analysis
        public string? PhysicalAppearance { get; set; }
        public decimal? MoisturePercentage { get; set; }
        public string? BacterialTest { get; set; }
        public string? FungalTest { get; set; }
        public string? MicroscopicAppearance { get; set; }
        public string? Color { get; set; }
        public string? Odor { get; set; }

        public AnalysisResults Result { get; set; }
        public string ResultNameEn { get; set; } = string.Empty;
        public string ResultNameAr { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }

        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}