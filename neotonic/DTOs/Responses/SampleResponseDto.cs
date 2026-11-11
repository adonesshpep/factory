using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class SampleResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public SampleTypes SampleType { get; set; }
        public DateTime SampleDate { get; set; }

        // Supplier + contract review (only set for supplier samples)
        public int? SupplierId { get; set; }
        public string? SupplierNumber { get; set; }
        public string? SupplierNameEn { get; set; }
        public string? SupplierNameAr { get; set; }

        public int? ContractReviewId { get; set; }
        public string? ContractReviewNumber { get; set; }

        // Material (derived: supplier -> ContractReview.RFQ.Material,
        //                    delivery -> Uom.Material)
        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        // Delivery-sample box (only set for delivery samples)
        public int? UomId { get; set; }
        public string? UomBarcode { get; set; }

        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}