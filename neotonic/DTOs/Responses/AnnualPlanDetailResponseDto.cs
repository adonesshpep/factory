// DTOs/AnnualPlanDetailResponseDto.cs
public class AnnualPlanDetailResponseDto
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public string MaterialNumber { get; set; } = string.Empty;
    public string MaterialNameEn { get; set; } = string.Empty;
    public string MaterialNameAr { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}