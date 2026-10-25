// Models/ProtocolStep.cs
// Delete behaviors (Cascade on Protocol, Restrict on Step) have no attribute
// equivalent — stays in OnModelCreating in the next step.
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("protocol_steps")]
    [Index(nameof(ProtocolId), nameof(OrderIndex), IsUnique = true, Name = "ix_protocol_steps_protocol_order")]
    public class ProtocolStep
    {
        public int Id { get; set; }

        public int ProtocolId { get; set; }

        [ForeignKey(nameof(ProtocolId))]
        public Protocol Protocol { get; set; } = null!;

        public int StepId { get; set; }

        [ForeignKey(nameof(StepId))]
        public Step Step { get; set; } = null!;

        [Column("order_index")]
        public int OrderIndex { get; set; }
    }
}