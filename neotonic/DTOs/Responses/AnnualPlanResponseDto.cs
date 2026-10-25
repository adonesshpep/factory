// DTOs/AnnualPlanResponseDto.cs
using neotonic.Models;

public class AnnualPlanResponseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentNameEn { get; set; } = string.Empty;
    public string DepartmentNameAr { get; set; } = string.Empty;
    public int MaterialTypeId { get; set; }
    public string MaterialTypeNameEn { get; set; } = string.Empty;
    public string MaterialTypeNameAr { get; set; } = string.Empty;
    public DateTime AnnualPlanDate { get; set; }
    public AnnualStatuses AnnualStatuses { get; set; }
    public string AnnualPlanStatusNameEn { get; set; } = string.Empty;
    public string AnnualPlanStatusNameAr { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public List<AnnualPlanDetailResponseDto> Details { get; set; } = new();
}