using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("supplier_return_orders")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_supplier_return_orders_number")]
    [Index(nameof(TransferOrderId), IsUnique = true, Name = "ix_supplier_return_orders_transfer_order")]
    public class SupplierReturnOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public int SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier Supplier { get; set; } = null!;

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