using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("rfqs")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_rfqs_number")]
    public class RFQ
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("creation_date")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public int? AnnualPlanID { get; set; }

        [ForeignKey(nameof(AnnualPlanID))]
        public AnnualPlan? AnnualPlan { get; set; }
        [Column("status")]
        public RFQStatus Status { get; set; } = RFQStatus.New;

        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
