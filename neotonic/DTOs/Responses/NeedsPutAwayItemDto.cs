using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class NeedsPutAwayItemDto
    {
        public int UomId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public UOMKinds Kind { get; set; }

        public int? MaterialId { get; set; }
        public string? MaterialNumber { get; set; }
        public string? MaterialNameEn { get; set; }
        public string? MaterialNameAr { get; set; }

        public int? ProductId { get; set; }
        public string? ProductNumber { get; set; }
        public string? ProductNameEn { get; set; }
        public string? ProductNameAr { get; set; }

        public decimal Quantity { get; set; }
        public int LocationId { get; set; }

        public string Reason { get; set; } = string.Empty;
        public int? SourceId { get; set; }
        public string? SourceNumber { get; set; }
    }
}