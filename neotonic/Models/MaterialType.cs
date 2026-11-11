// Models/MaterialType.cs
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("material_types")]
    [Index(nameof(NameEn), IsUnique = true)]
    [Index(nameof(NameAr), IsUnique = true)]
    public class MaterialType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;
        public int? FlowId { get; set; }

        [ForeignKey(nameof(FlowId))]
        public Flow? Flow { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}