// Models/WorkOrderDetailUOM.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("work_order_detail_uoms")]
    [Index(nameof(WorkOrderDetailId), nameof(UOMId), IsUnique = true, Name = "ix_work_order_detail_uoms_detail_uom")]
    public class WorkOrderDetailUOM
    {
        [Key]
        public int Id { get; set; }

        public int WorkOrderDetailId { get; set; }

        [ForeignKey(nameof(WorkOrderDetailId))]
        public WorkOrderDetail WorkOrderDetail { get; set; } = null!;

        public int UOMId { get; set; }

        [ForeignKey(nameof(UOMId))]
        public UOM Uom { get; set; } = null!;

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }
    }
}