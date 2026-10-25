using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace neotonic.DTOs.Creates
{
    public class CreateMaterialDto
    {
        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }

        [Required, MaxLength(50)] public string MaterialNumber { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string AccountingCode { get; set; } = string.Empty;

        [Required, Range(typeof(decimal), "0", "999999999999999")] public decimal? Min { get; set; }
        [Required, Range(typeof(decimal), "0", "999999999999999")] public decimal? Max { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }

        [Range(1, int.MaxValue)] public int CategoryId { get; set; }
        [Range(1, int.MaxValue)] public int MaterialTypeId { get; set; }
        [Required] public bool IsSerialized { get; set; }
    }
}
