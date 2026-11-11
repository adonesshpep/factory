// Models/ReleaseOrder.cs  (NEW)
// Business document: release a material to production (Prepared -> Released).
// Same shape as PreparingOrder: one material, many boxes, one TransferOrder. No protocol.
// Will be created from a manufacturing order (not modeled yet).
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("release_orders")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_release_orders_number")]
    public class ReleaseOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Manufacturing orders are not modeled yet - bare int, like UnitId elsewhere.
        public int? ManufacturingOrderId { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public int? TransferOrderId { get; set; }

        [ForeignKey(nameof(TransferOrderId))]
        public TransferOrder? TransferOrder { get; set; }
    }
}