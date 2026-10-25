// DTOs/SupplierResponseDto.cs
public class SupplierResponseDto
{
    public int Id { get; set; }
    public string SupplierNumber { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}