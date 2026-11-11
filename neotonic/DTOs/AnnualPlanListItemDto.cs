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

    // Rollup percentages (plan level; average of per-line %, each line capped at 100)
    public decimal RequestedPercentage { get; set; }
    public decimal ContractedPercentage { get; set; }
    public decimal OrderedPercentage { get; set; }
}