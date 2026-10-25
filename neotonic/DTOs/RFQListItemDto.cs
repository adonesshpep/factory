using neotonic.Models;

namespace neotonic.DTOs
{
    public class RFQListItemDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }

        public int? AnnualPlanID { get; set; }
        public string? AnnualPlanNumber { get; set; }

        public RFQStatus Status { get; set; }
        public string StatusNameEn { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;

        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public bool IsActive { get; set; }
    }
}