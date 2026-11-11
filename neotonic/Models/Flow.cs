using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("flows")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_flows_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_flows_name_ar")]
    public class Flow
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

        public ICollection<FlowStep> Steps { get; set; } = new List<FlowStep>();
    }
}
