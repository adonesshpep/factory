// DTOs/AnnualPlanDetailInputDto.cs
using System.ComponentModel.DataAnnotations;

public class AnnualPlanDetailInputDto
{
    [Required, Range(1, int.MaxValue)]
    public int MaterialId { get; set; }

    [Required, Range(typeof(decimal), "0.001", "999999999999999")]
    public decimal Quantity { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}