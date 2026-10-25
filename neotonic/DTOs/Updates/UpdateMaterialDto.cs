using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateMaterialDto
    {
        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }

        [MaxLength(50)] public string? MaterialNumber { get; set; }
        [MaxLength(50)] public string? AccountingCode { get; set; }

        [Range(typeof(decimal), "0", "999999999999999")] public decimal? Min { get; set; }
        [Range(typeof(decimal), "0", "999999999999999")] public decimal? Max { get; set; }

        public string? Notes { get; set; }

        [Range(1, int.MaxValue)] public int? CategoryId { get; set; }
        [Range(1, int.MaxValue)] public int? MaterialTypeId { get; set; }
        public bool? IsSerialized { get; set; }

    }
}
