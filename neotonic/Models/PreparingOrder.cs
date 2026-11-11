// Models/PreparingOrder.cs  (NEW)
// Business document: prepare a material (Accepted -> Prepared).
// Material, requested quantity, status and lines all live on its single TransferOrder.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("preparing_orders")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_preparing_orders_number")]
    public class PreparingOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Must be a protocol whose step type is "preparing" (matched by name, like the cleaning one).
        public int ProtocolId { get; set; }

        [ForeignKey(nameof(ProtocolId))]
        public Protocol Protocol { get; set; } = null!;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Exactly one, created in the same transaction as this order.
        public int? TransferOrderId { get; set; }

        [ForeignKey(nameof(TransferOrderId))]
        public TransferOrder? TransferOrder { get; set; }
    }
}