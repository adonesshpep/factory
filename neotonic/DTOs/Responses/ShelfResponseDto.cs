using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class ShelfResponseDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string? InventoryCode { get; set; }
        public InventoryZones? Zone { get; set; }
        public int Number { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOccupied { get; set; }
    }
}