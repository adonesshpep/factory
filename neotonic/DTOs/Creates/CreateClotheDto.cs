using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateClotheDto
    {
        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }
}
