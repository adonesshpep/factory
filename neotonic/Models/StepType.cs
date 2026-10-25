// Models/StepType.cs
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("step_types")]
    [Index(nameof(NameEn), IsUnique = true)]
    [Index(nameof(NameAr), IsUnique = true)]
    public class StepType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}