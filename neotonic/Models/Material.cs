// Models/Material.cs
// Note: HasPrecision(18,3) converts cleanly to Column(TypeName=...).
// The check constraint (ck_materials_min_max) has NO attribute equivalent —
// it stays in OnModelCreating in the next step.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("materials")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_materials_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_materials_name_ar")]
    [Index(nameof(MaterialNumber), IsUnique = true, Name = "ix_materials_material_number")]
    [Index(nameof(AccountingCode), IsUnique = true, Name = "ix_materials_accounting_code")]
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("material_number")]
        public string MaterialNumber { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("accounting_code")]
        public string AccountingCode { get; set; } = string.Empty;

        [Column("min", TypeName = "decimal(18,3)")]
        public decimal Min { get; set; }

        [Column("max", TypeName = "decimal(18,3)")]
        public decimal Max { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        public int MaterialTypeId { get; set; }

        [ForeignKey(nameof(MaterialTypeId))]
        public MaterialType MaterialType { get; set; } = null!;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_serialized")]
        public bool IsSerialized { get; set; } = true;
    }
}