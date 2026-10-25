// DTOs/MemoDetailResponseDto.cs
namespace neotonic.DTOs
{
    public class MemoDetailResponseDto
    {
        public int Id { get; set; }
        public int MemoId { get; set; }

        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;
        public bool MaterialIsSerialized { get; set; }

        public decimal Quantity { get; set; }
        public decimal SplitQuantity { get; set; }

        public string? Lot { get; set; }
        public string? FactoryName { get; set; }
        public string? CountryOrigin { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? UnitId { get; set; }
        public string? Notes { get; set; }

        public List<UOMResponseDto> UOMs { get; set; } = new();
    }
}
