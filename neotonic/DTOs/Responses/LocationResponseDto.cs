using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class LocationResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public LocationKinds Kind { get; set; }
        public int? InventoryId { get; set; }
        public string? InventoryCode { get; set; }
        public InventoryZones? Zone { get; set; }
    }
}