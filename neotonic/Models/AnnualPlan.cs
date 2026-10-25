// Models/AnnualPlan.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("annual_plans")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_annual_plans_number")]
    public class AnnualPlan
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; } = null!;

        public int MaterialTypeId { get; set; }

        [ForeignKey(nameof(MaterialTypeId))]
        public MaterialType MaterialType { get; set; } = null!;

        [Column("annual_plan_date")]
        public DateTime AnnualPlanDate { get; set; } = DateTime.UtcNow;

        [Column("annual_statuses")]
        public AnnualStatuses AnnualStatuses { get; set; } = AnnualStatuses.New;

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<AnnualPlanDetail> Details { get; set; } = new List<AnnualPlanDetail>();
    }
}