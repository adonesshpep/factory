// Models/MemoDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("memo_details")]
    [Index(nameof(MemoId), nameof(MaterialId), Name = "ix_memo_details_memo_material")]
    public class MemoDetail
    {
        [Key]
        public int Id { get; set; }

        public int MemoId { get; set; }

        [ForeignKey(nameof(MemoId))]
        public Memo Memo { get; set; } = null!;

        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column("split_quantity", TypeName = "decimal(18,3)")]
        public decimal SplitQuantity { get; set; }

        [MaxLength(100)]
        [Column("lot")]
        public string? Lot { get; set; }

        [MaxLength(200)]
        [Column("factory_name")]
        public string? FactoryName { get; set; }

        [MaxLength(200)]
        [Column("country_origin")]
        public string? CountryOrigin { get; set; }

        [Column("production_date")]
        public DateTime? ProductionDate { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        // Unit is not modeled in neotonic — kept as a bare int.
        public int? UnitId { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        public ICollection<MemoDetailUOM> UOMs { get; set; } = new List<MemoDetailUOM>();
    }
}