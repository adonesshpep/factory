using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("scrap_orders")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_scrap_orders_number")]
    [Index(nameof(TransferOrderId), IsUnique = true, Name = "ix_scrap_orders_transfer_order")]
    public class ScrapOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

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