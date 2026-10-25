// Models/MemoRWDetail.cs
// Barcode is sent by the client (scanned from the physical label) and must be
// globally unique — the unique index enforces it at the DB level, and the
// controller maps the SqlException to a friendly Conflict message.
// No invariant between AWeight and BWeight: A is taken after removing the box,
// B is before, so A < B is the normal case but it is not enforced.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("memo_rw_details")]
    [Index(nameof(Barcode), IsUnique = true, Name = "ix_memo_rw_details_barcode")]
    public class MemoRWDetail
    {
        [Key]
        public int Id { get; set; }

        public int MemoId { get; set; }

        [ForeignKey(nameof(MemoId))]
        public MemoRWIn Memo { get; set; } = null!;

        [Required]
        [Column("barcode")]
        public long Barcode { get; set; }

        [Required]
        [Column("aweight", TypeName = "decimal(18,3)")]
        public decimal AWeight { get; set; }

        [Required]
        [Column("bweight", TypeName = "decimal(18,3)")]
        public decimal BWeight { get; set; }
    }
}