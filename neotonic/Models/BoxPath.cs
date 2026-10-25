// Models/BoxPath.cs
// NEW entity — no prior Fluent config existed for this one. I'm applying the
// same bilingual-unique pattern used elsewhere as a guess; confirm Code should
// actually be unique before this ships.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("box_paths")]
    [Index(nameof(Code), IsUnique = true)]
    public class BoxPath
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}