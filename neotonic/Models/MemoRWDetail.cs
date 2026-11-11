// Models/MemoRWDetail.cs  (CHANGED)
// One received box. UomId + Tear only; B weight = UOM.Quantity + Tear, never stored.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("memo_rw_details")]
    [Index(nameof(UomId), IsUnique = true, Name = "ix_memo_rw_details_uom")]
    public class MemoRWDetail
    {
        [Key]
        public int Id { get; set; }

        public int MemoId { get; set; }

        [ForeignKey(nameof(MemoId))]
        public MemoRWIn Memo { get; set; } = null!;

        public int UomId { get; set; }

        [ForeignKey(nameof(UomId))]
        public UOM Uom { get; set; } = null!;

        [Column("tear", TypeName = "decimal(18,3)")]
        public decimal Tear { get; set; }
    }
}