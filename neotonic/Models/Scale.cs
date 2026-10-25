// Models/Scale.cs
// NEW entity — same situation as BoxPath; no prior Fluent config to match
// against. Applying the Machine pattern since the shape is identical.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("scales")]
    [Index(nameof(ScaleNumber), IsUnique = true, Name = "ix_scales_scale_number")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_scales_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_scales_name_ar")]
    public class Scale
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("scale_number")]
        public string ScaleNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; } = null!;

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}