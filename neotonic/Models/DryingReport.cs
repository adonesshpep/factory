using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("drying_reports")]
    public class DryingReport
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

        [Column("humidity_before_percentage", TypeName = "decimal(5,2)")] public decimal HumidityBeforePercentage { get; set; }
        [Column("humidity_after_percentage", TypeName = "decimal(5,2)")] public decimal HumidityAfterPercentage { get; set; }
        [Column("duration")] public int Duration { get; set; }                                   // minutes
        [MaxLength(200)][Column("drying_kind")] public string DryingKind { get; set; } = string.Empty;
        [Column("temperature", TypeName = "decimal(6,2)")] public decimal Temperature { get; set; }

        public int SampleId { get; set; }
        [ForeignKey(nameof(SampleId))] public Sample Sample { get; set; } = null!;

        public int WorkerId { get; set; }
        [ForeignKey(nameof(WorkerId))] public Worker Worker { get; set; } = null!;

        [MaxLength(1000)][Column("notes")] public string? Notes { get; set; }
        [MaxLength(450)][Column("user_id")] public string UserId { get; set; } = string.Empty;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}