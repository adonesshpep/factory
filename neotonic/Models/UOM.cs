// Models/UOM.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Table("uoms")]
    [Index(nameof(Barcode), IsUnique = true, Name = "ix_uoms_barcode")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_uoms_number")]
    public class UOM
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("barcode")]
        public string Barcode { get; set; } = string.Empty;

        [Column("number")]
        public long Number { get; set; }

        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;

        [MaxLength(100)]
        [Column("lot")]
        public string? Lot { get; set; }

        [Column("box_number")]
        public int BoxNumber { get; set; }

        [Column("print_count")]
        public int PrintCount { get; set; }

        [Column("quantity_in", TypeName = "decimal(18,3)")]
        public decimal QuantityIn { get; set; }

        [Column("quantity_out", TypeName = "decimal(18,3)")]
        public decimal QuantityOut { get; set; }

        [Column("quantity_work_order", TypeName = "decimal(18,3)")]
        public decimal QuantityWorkOrder { get; set; }
    }
}