// Models/WorkOrderDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("work_order_details")]
    public class WorkOrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int WorkOrderId { get; set; }

        [ForeignKey(nameof(WorkOrderId))]
        public WorkOrder WorkOrder { get; set; } = null!;

        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;

        // Unit is not modeled in neotonic — kept as a bare int.
        public int? UnitId { get; set; }

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [MaxLength(200)]
        [Column("notes")]
        public string? Notes { get; set; }

        public ICollection<WorkOrderDetailUOM> UOMs { get; set; } = new List<WorkOrderDetailUOM>();
    }
}