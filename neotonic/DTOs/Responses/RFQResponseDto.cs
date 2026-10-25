using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class RFQResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }

        // Annual plan (nullable)
        public int? AnnualPlanID { get; set; }
        public string? AnnualPlanNumber { get; set; }
        public DateTime? AnnualPlanDate { get; set; }

        public RFQStatus Status { get; set; }
        public string StatusNameEn { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;

        // Material
        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}