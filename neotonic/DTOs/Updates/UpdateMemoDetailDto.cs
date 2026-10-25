// DTOs/UpdateMemoDetailDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class UpdateMemoDetailDto
    {
        // 0 = new row, > 0 = update existing.
        public int Id { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [Required, Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal Quantity { get; set; }

        [Range(typeof(decimal), "0", "999999999999999")]
        public decimal SplitQuantity { get; set; }

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
    }
}