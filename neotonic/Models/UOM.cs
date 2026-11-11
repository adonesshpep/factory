// Models/UOM.cs  (FINAL)
// One UOM = one physical box with one barcode. Kind decides whether it holds a material or a product;
// exactly one of MaterialId/ProductId is set (check constraint).
// Reservation is NOT a column: it is the sum of Done transfer order lines for this box.
// The shelf (if any) must belong to the box's own Location — enforced by a composite FK to
// Shelf(Id, LocationId) configured in AppDbContext, not here.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("uoms")]
    [Index(nameof(Barcode), IsUnique = true, Name = "ix_uoms_barcode")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_uoms_number")]
    [Index(nameof(LocationId), nameof(MaterialId), Name = "ix_uoms_location_material")]
    [Index(nameof(LocationId), nameof(ProductId), Name = "ix_uoms_location_product")]
    [Index(nameof(ParentUomId), Name = "ix_uoms_parent_uom")]
    public class UOM
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("barcode")]
        public string Barcode { get; set; } = string.Empty;

        [Column("number")]
        public long Number { get; set; }

        [Column("kind")]
        public UOMKinds Kind { get; set; } = UOMKinds.Material;

        // Exactly one of MaterialId / ProductId, matching Kind (ck_uoms_kind_link).
        public int? MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material? Material { get; set; }

        public int? ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [MaxLength(100)]
        [Column("lot")]
        public string? Lot { get; set; }

        [Column("box_number")]
        public int BoxNumber { get; set; }

        [Column("print_count")]
        public int PrintCount { get; set; }

        // The A / net weight. B weight = Quantity + Tear (on MemoRWDetail), never stored.
        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        // ---- where the box is ----
        public int LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; } = null!;

        // Nullable: a box may have no shelf (displayed as zero).
        // Composite FK (ShelfId, LocationId) -> Shelf(Id, LocationId) is configured in AppDbContext.
        public int? ShelfId { get; set; }

        public Shelf? Shelf { get; set; }

        // ---- split: set on the child box cut out of another box ----
        public int? ParentUomId { get; set; }

        [ForeignKey(nameof(ParentUomId))]
        public UOM? Parent { get; set; }

        public ICollection<UOM> Children { get; set; } = new List<UOM>();

        // ---- receiving side ----
        public MemoRWDetail? MemoRWDetail { get; set; }

        public ICollection<Sample> Samples { get; set; } = new List<Sample>();
    }
}