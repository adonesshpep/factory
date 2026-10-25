using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("scale_equalizations")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_scale_equalizations_number")]
    public class ScaleEqualization
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("equalization_date")]
        public DateTime EqualizationDate { get; set; }

        public int ScaleId { get; set; }

        [ForeignKey(nameof(ScaleId))]
        public Scale Scale { get; set; } = null!;

        public int WorkerId { get; set; }

        [ForeignKey(nameof(WorkerId))]
        public Worker Worker { get; set; } = null!;

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }
    }
}