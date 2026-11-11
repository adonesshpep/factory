using Microsoft.EntityFrameworkCore;
using neotonic.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace neotonic.Models
{

    [Table("flow_steps")]
    [Index(nameof(FlowId), nameof(OrderIndex), IsUnique = true, Name = "ix_flow_steps_flow_order")]
    public class FlowStep
    {
        [Key]
        public int Id { get; set; }

        public int FlowId { get; set; }

        [ForeignKey(nameof(FlowId))]
        public Flow Flow { get; set; } = null!;

        public int LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; } = null!;

        [Column("order_index")]
        public int OrderIndex { get; set; }
    }
}