using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("preparing_order_transfer_links")]
    [Index(nameof(PreparingOrderId), nameof(StepOrderIndex), Name = "ix_prep_link_order_step")]
    [Index(nameof(TransferOrderId), IsUnique = true, Name = "ix_prep_link_transfer")]
    public class PreparingOrderTransferLink
    {
        [Key]
        public int Id { get; set; }

        public int PreparingOrderId { get; set; }

        [ForeignKey(nameof(PreparingOrderId))]
        public PreparingOrder PreparingOrder { get; set; } = null!;

        public int TransferOrderId { get; set; }

        [ForeignKey(nameof(TransferOrderId))]
        public TransferOrder TransferOrder { get; set; } = null!;

        [Column("step_order_index")]
        public int StepOrderIndex { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}