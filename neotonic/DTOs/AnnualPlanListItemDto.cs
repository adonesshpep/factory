// DTOs/AnnualPlanListItemDto.cs
using neotonic.Models;

public class AnnualPlanListItemDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string DepartmentNameEn { get; set; } = string.Empty;
    public string DepartmentNameAr { get; set; } = string.Empty;
    public string MaterialTypeNameEn { get; set; } = string.Empty;
    public string MaterialTypeNameAr { get; set; } = string.Empty;
    public DateTime AnnualPlanDate { get; set; }
    public AnnualStatuses AnnualStatuses { get; set; }
    public string AnnualPlanStatusNameEn { get; set; } = string.Empty;
    public string AnnualPlanStatusNameAr { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DetailCount { get; set; }
}