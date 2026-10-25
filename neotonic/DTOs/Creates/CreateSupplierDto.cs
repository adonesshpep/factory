// DTOs/CreateSupplierDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateSupplierDto
{
    [Required, MaxLength(50)]
    public string SupplierNumber { get; set; } = string.Empty;

    [MaxLength(200)] public string? NameEn { get; set; }
    [MaxLength(200)] public string? NameAr { get; set; }

    [MaxLength(200)] public string? Contact { get; set; }
    [MaxLength(30)] public string? PhoneNumber { get; set; }
    [EmailAddress, MaxLength(200)] public string? Email { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
}