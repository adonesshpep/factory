// DTOs/CreateMemoOutDetailDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class CreateMemoOutDetailDto
    {
        [Required, Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [Required, Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal Quantity { get; set; }

        [MaxLength(100)]
        public string? Lot { get; set; }

        [MaxLength(200)]
        public string? FactoryName { get; set; }

        [MaxLength(200)]
        public string? CountryOrigin { get; set; }

        public DateTime? ProductionDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? UnitId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Which physical UOMs are being taken out. Required for Out memos.
        [Required, MinLength(1)]
        public List<MemoOutUOMInputDto> UOMs { get; set; } = new();
    }
}