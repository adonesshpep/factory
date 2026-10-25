// Models/Memo.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("memos")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_memos_number")]
    public class Memo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("number")]
        public long Number { get; set; }

        [Column("memo_date")]
        public DateTime MemoDate { get; set; } = DateTime.UtcNow;

        [Column("creation_date")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        // Stored as int. See MemoType enum: In = 1, Out = -1.
        [Column("type_id")]
        public MemoType TypeId { get; set; }

        [MaxLength(50)]
        [Column("invoice_number")]
        public string? InvoiceNumber { get; set; }

        [Column("invoice_date")]
        public DateTime? InvoiceDate { get; set; }

        // Nullable — Out memos have no supplier
        public int? SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier? Supplier { get; set; }

        // Nullable — not every memo is tied to a work order
        public int? WorkOrderId { get; set; }

        [ForeignKey(nameof(WorkOrderId))]
        public WorkOrder? WorkOrder { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<MemoDetail> Details { get; set; } = new List<MemoDetail>();
    }
}