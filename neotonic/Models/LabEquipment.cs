// Models/LabEquipment.cs
// No unique constraints here, matching the original — intentional, not an
// oversight, per our earlier discussion.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("lab_equipments")]
    public class LabEquipment
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("job_en")]
        public string JobEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("job_ar")]
        public string JobAr { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("manufacturer")]
        public string Manufacturer { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("model")]
        public string Model { get; set; } = string.Empty;

        [Column("equalizing_date")]
        public DateTime EqualizingDate { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}