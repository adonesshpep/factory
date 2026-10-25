// Models/WorkOrder.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("work_orders")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_work_orders_number")]
    public class WorkOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("number")]
        public long Number { get; set; }

        [Column("work_order_date")]
        public DateTime WorkOrderDate { get; set; } = DateTime.UtcNow;

        [Column("batch_size", TypeName = "decimal(18,3)")]
        public decimal BatchSize { get; set; }

        [Column("weight", TypeName = "decimal(18,3)")]
        public decimal Weight { get; set; }

        [MaxLength(20)]
        [Column("lot")]
        public string? Lot { get; set; }

        [Column("production_date")]
        public DateTime ProductionDate { get; set; }

        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(100)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("status_id")]
        public WorkStatus StatusId { get; set; } = WorkStatus.Pending;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<WorkOrderDetail> Details { get; set; } = new List<WorkOrderDetail>();
    }
}