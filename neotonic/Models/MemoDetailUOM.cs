// Models/MemoDetailUOM.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("memo_detail_uoms")]
    [Index(nameof(MemoDetailId), nameof(UOMId), IsUnique = true, Name = "ix_memo_detail_uoms_detail_uom")]
    public class MemoDetailUOM
    {
        [Key]
        public int Id { get; set; }

        public int MemoDetailId { get; set; }

        [ForeignKey(nameof(MemoDetailId))]
        public MemoDetail MemoDetail { get; set; } = null!;

        public int UOMId { get; set; }

        [ForeignKey(nameof(UOMId))]
        public UOM Uom { get; set; } = null!;

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }
    }
}