// Models/StockMove.cs  (CHANGED - adds FromShelfId / ToShelfId)
// Append-only. One row per moved box; a split writes one row for the child box.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("stock_moves")]
    [Index(nameof(UomId), nameof(MoveDate), Name = "ix_stock_moves_uom_date")]
    [Index(nameof(SourceType), nameof(SourceId), Name = "ix_stock_moves_source")]
    public class StockMove
    {
        [Key]
        public int Id { get; set; }

        public int UomId { get; set; }

        [ForeignKey(nameof(UomId))]
        public UOM Uom { get; set; } = null!;

        public int FromLocationId { get; set; }

        [ForeignKey(nameof(FromLocationId))]
        public Location FromLocation { get; set; } = null!;

        public int ToLocationId { get; set; }

        [ForeignKey(nameof(ToLocationId))]
        public Location ToLocation { get; set; } = null!;

        // Nullable shelves: a box may move from/to "no shelf".
        public int? FromShelfId { get; set; }

        [ForeignKey(nameof(FromShelfId))]
        public Shelf? FromShelf { get; set; }

        public int? ToShelfId { get; set; }

        [ForeignKey(nameof(ToShelfId))]
        public Shelf? ToShelf { get; set; }

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column("move_type")]
        public StockMoveTypes MoveType { get; set; } = StockMoveTypes.Move;

        [Column("source_type")]
        public StockMoveSources SourceType { get; set; } = StockMoveSources.None;

        [Column("source_id")]
        public int? SourceId { get; set; }

        [MaxLength(500)]
        [Column("reason")]
        public string? Reason { get; set; }

        [Column("move_date")]
        public DateTime MoveDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(450)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;
    }
}