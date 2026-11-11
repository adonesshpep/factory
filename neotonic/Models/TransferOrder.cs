// Models/TransferOrder.cs  (NEW) - THE ENGINE OF THE INVENTORY SYSTEM
// "Move these boxes of ONE material from location A to location B."
// It knows nothing about protocols, numbers or manufacturing. It is created by a business order
// (PreparingOrder or ReleaseOrder) in the same transaction, never directly by the user.
//
// Lifecycle:  New --Done--> Done --Finish--> Executed      (Cancelled from New or Done)
//   New       : lines saved, nothing reserved, editable
//   Done      : the lines now count as reservations on their boxes (nothing is written on the UOM)
//   Executed  : scanned boxes moved to ToLocation (a line for less than the whole box splits it first)
//
// The business orders have no status of their own: they show this one.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("transfer_orders")]
    [Index(nameof(Status), Name = "ix_transfer_orders_status")]
    public class TransferOrder
    {
        [Key]
        public int Id { get; set; }

        public int FromLocationId { get; set; }

        [ForeignKey(nameof(FromLocationId))]
        public Location FromLocation { get; set; } = null!;

        public int ToLocationId { get; set; }

        [ForeignKey(nameof(ToLocationId))]
        public Location ToLocation { get; set; } = null!;

        // One material per transfer order. Every line's box must be of this material.
        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;

        // The requested amount. A target only: lines are not forced to add up to it.
        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column("status")]
        public TransferOrderStatuses Status { get; set; } = TransferOrderStatuses.New;

        [Column("executed_at")]
        public DateTime? ExecutedAt { get; set; }

        // ---- the business order that owns this transfer: exactly one of the two is set ----


        public ICollection<TransferOrderLine> Lines { get; set; } = new List<TransferOrderLine>();
    }
}