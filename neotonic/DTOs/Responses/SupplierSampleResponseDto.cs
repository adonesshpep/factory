namespace neotonic.DTOs.Responses
{
    public class SupplierSampleResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }

        public int SupplierId { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;
        public string SupplierNameEn { get; set; } = string.Empty;
        public string SupplierNameAr { get; set; } = string.Empty;

        public int ContractReviewId { get; set; }
        public string ContractReviewNumber { get; set; } = string.Empty;

        // Read from the contract review, not stored on the sample
        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}