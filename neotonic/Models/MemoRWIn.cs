// Models/MemoRWIn.cs
// Number is server-generated via the memo_rw_in_number_seq sequence (see AppDbContext),
// so concurrent requests can never collide like the desktop's max(Number)+1 did.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("memo_rw_in")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_memo_rw_in_number")]
    public class MemoRWIn
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("number")]
        public long Number { get; set; }

        [Column("memo_date")]
        public DateTime MemoDate { get; set; } = DateTime.UtcNow;

        public int PurchaseOrderId { get; set; }

        [ForeignKey(nameof(PurchaseOrderId))]
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        public int WorkerId { get; set; }

        [ForeignKey(nameof(WorkerId))]
        public Worker Worker { get; set; } = null!;

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<MemoRWDetail> Details { get; set; } = new List<MemoRWDetail>();
    }
}