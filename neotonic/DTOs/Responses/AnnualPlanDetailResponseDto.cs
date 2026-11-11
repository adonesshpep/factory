public class AnnualPlanDetailResponseDto
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public string MaterialNumber { get; set; } = string.Empty;
    public string MaterialNameEn { get; set; } = string.Empty;
    public string MaterialNameAr { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }

    // Rollup per line (uncapped percentages so over-fulfilment shows)
    public decimal RequestedQuantity { get; set; }
    public decimal ContractedQuantity { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal RequestedPercentage { get; set; }
    public decimal ContractedPercentage { get; set; }
    public decimal OrderedPercentage { get; set; }
}