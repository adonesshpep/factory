// Models/AnnualPlanDetail.cs
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("annual_plan_details")]
    [Index(nameof(AnnualPlanId), nameof(MaterialId), IsUnique = true, Name = "ix_annual_plan_details_plan_material")]
    public class AnnualPlanDetail
    {
        public int Id { get; set; }

        public int AnnualPlanId { get; set; }

        [ForeignKey(nameof(AnnualPlanId))]
        public AnnualPlan AnnualPlan { get; set; } = null!;

        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }
    }
}