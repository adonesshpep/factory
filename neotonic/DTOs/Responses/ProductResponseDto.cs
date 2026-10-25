namespace neotonic.DTOs.Responses
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string ProductNumber { get; set; } = string.Empty;
        public string AccountingCode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int CategoryId { get; set; }
        public string CategoryNameEn { get; set; } = string.Empty;
        public string CategoryNameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSerialized { get; set; }
    }
}
