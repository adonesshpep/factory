// Models/Step.cs
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("steps")]
    [Index(nameof(StepNumber), IsUnique = true, Name = "ix_steps_step_number")]
    [Index(nameof(StepNameEn), IsUnique = true, Name = "ix_steps_step_name_en")]
    [Index(nameof(StepNameAr), IsUnique = true, Name = "ix_steps_step_name_ar")]
    public class Step
    {
        [Key]
        public int Id { get; set; }

        [Column("step_number")]
        public int StepNumber { get; set; }

        [Required, MaxLength(200)]
        [Column("step_name_en")]
        public string StepNameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("step_name_ar")]
        public string StepNameAr { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        public int? BoxPathId { get; set; }

        [ForeignKey(nameof(BoxPathId))]
        public BoxPath? BoxPath { get; set; }

        [Column("color")]
        public string? Color { get; set; } = "#FFFFFF";
        public int? VirtualLocationId { get; set; }

        [ForeignKey(nameof(VirtualLocationId))]
        public Location? VirtualLocation { get; set; }

        public int StepTypeId { get; set; }

        [ForeignKey(nameof(StepTypeId))]
        public StepType StepType { get; set; } = null!;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}