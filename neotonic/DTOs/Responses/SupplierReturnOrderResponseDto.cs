namespace neotonic.DTOs.Responses
{
    public class SupplierReturnOrderResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }

        public int SupplierId { get; set; }
        public string SupplierNumber { get; set; } = string.Empty;
        public string SupplierNameEn { get; set; } = string.Empty;
        public string SupplierNameAr { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public TransferOrderResponseDto? TransferOrder { get; set; }
    }
}