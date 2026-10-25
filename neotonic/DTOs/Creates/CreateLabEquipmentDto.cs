using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateLabEquipmentDto
    {
        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }

        [MaxLength(200)] public string? JobEn { get; set; }
        [MaxLength(200)] public string? JobAr { get; set; }

        [Required, MaxLength(200)] public string Manufacturer { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Model { get; set; } = string.Empty;

        [Required] public DateTime? EqualizingDate { get; set; }
    }
}
