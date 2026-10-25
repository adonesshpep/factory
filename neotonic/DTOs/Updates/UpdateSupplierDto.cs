// DTOs/UpdateSupplierDto.cs
using System.ComponentModel.DataAnnotations;

public class UpdateSupplierDto
{
    public string? SupplierNumber { get; set; }
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? Contact { get; set; }
    public string? PhoneNumber { get; set; }
    [EmailAddress] public string? Email { get; set; }
    public string? Notes { get; set; }
}