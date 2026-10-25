// Models/Machine.cs
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("machines")]
    [Index(nameof(MachineNumber), IsUnique = true, Name = "ix_machines_machine_number")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_machines_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_machines_name_ar")]
    public class Machine
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("machine_number")]
        public string MachineNumber { get; set; } = string.Empty;

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