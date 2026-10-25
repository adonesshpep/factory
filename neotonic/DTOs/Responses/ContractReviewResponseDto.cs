using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class ContractReviewResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public DateTime ExpectedDate { get; set; }

        public int SupplierId { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;
        public string SupplierNameEn { get; set; } = string.Empty;
        public string SupplierNameAr { get; set; } = string.Empty;

        public ConnectionMethods ConnectionMethod { get; set; }
        public string ConnectionMethodNameEn { get; set; } = string.Empty;
        public string ConnectionMethodNameAr { get; set; } = string.Empty;

        public CRStatus Status { get; set; }
        public string StatusNameEn { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;

        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;
        public int RFQId { get; set; }
        public string RFQNumber { get; set; } = string.Empty;
        public RFQStatus RFQStatus { get; set; }
        public string RFQStatusNameEn { get; set; } = string.Empty;
        public string RFQStatusNameAr { get; set; } = string.Empty;
        public decimal RFQQuantity { get; set; }
        public decimal RFQFulfilledQuantity { get; set; }
        public decimal RFQRemainingQuantity { get; set; }
        public decimal RFQFulfillmentPercentage { get; set; }
        public bool RFQIsFulfilled { get; set; }
        public decimal Quantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal FulfillmentPercentage { get; set; }   // can exceed 100
        public bool IsFulfilled { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}