using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("clothes")]
    public class Clothe
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Notes { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

    }
}
