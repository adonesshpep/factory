using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("sterilization_reports")]
    public class SterilizationReport
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

        [Column("duration")] public int Duration { get; set; }                                   // minutes
        [Column("wavelength", TypeName = "decimal(8,3)")] public decimal Wavelength { get; set; }  // nm
        [Column("thickness", TypeName = "decimal(8,3)")] public decimal Thickness { get; set; }    // cm
        [Column("speed", TypeName = "decimal(8,3)")] public decimal Speed { get; set; }
        [Column("temperature", TypeName = "decimal(6,2)")] public decimal Temperature { get; set; }
        [Column("humidity_percentage", TypeName = "decimal(5,2)")] public decimal HumidityPercentage { get; set; }

        public int SampleId { get; set; }
        [ForeignKey(nameof(SampleId))] public Sample Sample { get; set; } = null!;

        public int WorkerId { get; set; }
        [ForeignKey(nameof(WorkerId))] public Worker Worker { get; set; } = null!;

        [MaxLength(1000)][Column("notes")] public string? Notes { get; set; }
        [MaxLength(450)][Column("user_id")] public string UserId { get; set; } = string.Empty;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}