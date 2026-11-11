// Models/TransferOrderLine.cs  (CHANGED)
// One box on a transfer order, and the reservation ledger entry for it.
// Material, lot and barcode come from the UOM, nothing is copied here.
//
// While the transfer order is Done, this line holds Quantity of the box (see UOM.cs for the formulas).
// New, Executed and Cancelled orders hold nothing.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("transfer_order_lines")]
    [Index(nameof(TransferOrderId), nameof(UomId), IsUnique = true, Name = "ix_transfer_order_lines_order_uom")]
    [Index(nameof(UomId), Name = "ix_transfer_order_lines_uom")]   // reserved-quantity lookups per box
    public class TransferOrderLine
    {
        [Key]
        public int Id { get; set; }

        public int TransferOrderId { get; set; }

        [ForeignKey(nameof(TransferOrderId))]
        public TransferOrder TransferOrder { get; set; } = null!;

        public int UomId { get; set; }

        [ForeignKey(nameof(UomId))]
        public UOM Uom { get; set; } = null!;

        // How much of the box this order takes. Always set: the front end defaults it to the box's
        // available quantity, so "the whole box" is just Quantity = box quantity.
        // Validated in the service: > 0 and <= available at the time of Done.
        // At Finish: Quantity = box quantity -> the box moves whole; less -> the box is split first.
        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        // Set at Finish for the lines whose barcode was scanned.
        [Column("is_scanned")]
        public bool IsScanned { get; set; }
    }
}