using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("purification_reports")]
    public class PurificationReport
    {
        [Key] public int Id { get; set; }

        public int PreparingOrderId { get; set; }
        [ForeignKey(nameof(PreparingOrderId))] public PreparingOrder PreparingOrder { get; set; } = null!;

        public int UomId { get; set; }
        [ForeignKey(nameof(UomId))] public UOM Uom { get; set; } = null!;

        public int StockMoveId { get; set; }
        [ForeignKey(nameof(StockMoveId))] public StockMove StockMove { get; set; } = null!;

        [Column("before_weight", TypeName = "decimal(18,3)")] public decimal BeforeWeight { get; set; }
        [Column("after_weight", TypeName = "decimal(18,3)")] public decimal AfterWeight { get; set; }
        [Column("loss_percentage", TypeName = "decimal(5,2)")] public decimal LossPercentage { get; set; }

        [Column("theoretical_impurity_weight", TypeName = "decimal(18,3)")] public decimal TheoreticalImpurityWeight { get; set; }
        [Column("practical_impurity_weight", TypeName = "decimal(18,3)")] public decimal PracticalImpurityWeight { get; set; }

        public int WorkerId { get; set; }
        [ForeignKey(nameof(WorkerId))] public Worker Worker { get; set; } = null!;

        [MaxLength(1000)][Column("notes")] public string? Notes { get; set; }
        [MaxLength(450)][Column("user_id")] public string UserId { get; set; } = string.Empty;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}