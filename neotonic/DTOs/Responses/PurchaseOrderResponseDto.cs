using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class PurchaseOrderResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal Quantity { get; set; }

        public POStatus Status { get; set; }
        public string StatusNameEn { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;

        // Certificate
        public int CertificateOfAnalysisId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime CertificateAnalysisDate { get; set; }
        public AnalysisResults CertificateResult { get; set; }
        public string CertificateResultNameEn { get; set; } = string.Empty;
        public string CertificateResultNameAr { get; set; } = string.Empty;

        // Contract review + fulfilment
        public int ContractReviewId { get; set; }
        public string ContractReviewNumber { get; set; } = string.Empty;
        public decimal ContractReviewQuantity { get; set; }
        public decimal ContractReviewFulfilledQuantity { get; set; }
        public decimal ContractReviewRemainingQuantity { get; set; }
        public decimal ContractReviewFulfillmentPercentage { get; set; }
        public bool ContractReviewIsFulfilled { get; set; }

        // Supplier and material (from the contract review)
        public int SupplierId { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;
        public string SupplierNameEn { get; set; } = string.Empty;
        public string SupplierNameAr { get; set; } = string.Empty;

        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}