// DTOs/UpdateAnnualPlanDto.cs
public class UpdateAnnualPlanDto
{
    public string? Number { get; set; }
    public int? DepartmentId { get; set; }
    public int? MaterialTypeId { get; set; }
    public DateTime? AnnualPlanDate { get; set; }
    public string? Notes { get; set; }

    // null = leave existing details unchanged. A provided list (even empty)
    // fully replaces the current details, same pattern as ProtocolSteps.
    public List<AnnualPlanDetailInputDto>? Details { get; set; }
}