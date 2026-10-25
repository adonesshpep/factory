// DTOs/CreateAnnualPlanDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateAnnualPlanDto
{
    [Required, MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaterialTypeId { get; set; }

    public DateTime? AnnualPlanDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<AnnualPlanDetailInputDto> Details { get; set; } = new();
}