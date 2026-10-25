using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateProductDto
    {
            [MaxLength(200)] public string? NameEn { get; set; }
            [MaxLength(200)] public string? NameAr { get; set; }

            [Required, MaxLength(50)] public string ProductNumber { get; set; } = string.Empty;
            [Required, MaxLength(50)] public string AccountingCode { get; set; } = string.Empty;


            [MaxLength(1000)] public string? Notes { get; set; }

            [Range(1, int.MaxValue)] public int CategoryId { get; set; }
            [Required] public bool IsSerialized { get; set; }
        }
}
