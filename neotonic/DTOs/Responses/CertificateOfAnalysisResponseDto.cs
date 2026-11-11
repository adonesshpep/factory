using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class CertificateOfAnalysisResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }

        // Sample (both kinds)
        public int SampleId { get; set; }
        public string SampleNumber { get; set; } = string.Empty;
        public SampleTypes SampleType { get; set; }
        public DateTime SampleDate { get; set; }

        // Supplier (only set for supplier samples; null-safe read of Sample.ContractReview.Supplier)
        public int? SupplierId { get; set; }
        public string? SupplierNumber { get; set; }
        public string? SupplierNameEn { get; set; }
        public string? SupplierNameAr { get; set; }

        // Contract review (only set for supplier samples)
        public int? ContractReviewId { get; set; }
        public string? ContractReviewNumber { get; set; }
        public DateTime? ContractReviewCreationDate { get; set; }
        public DateTime? ContractReviewExpectedDate { get; set; }
        public decimal? ContractReviewQuantity { get; set; }
        public CRStatus? ContractReviewStatus { get; set; }
        public string? ContractReviewStatusNameEn { get; set; }
        public string? ContractReviewStatusNameAr { get; set; }

        // Material (derived: supplier sample -> Sample.ContractReview.RFQ.Material
        //                      delivery sample -> Sample.Uom.Material)
        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        // Delivery-sample box (only set for delivery samples)
        public int? UomId { get; set; }
        public string? UomBarcode { get; set; }

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

        // Result-route extras (populated only by PUT result/{id}; null/false on GET)
        public bool? BoxMoved { get; set; }
        public int? BoxNewLocationId { get; set; }
    }
}