using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("cleaning_requests")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_cleaning_requests_number")]
    public class CleaningRequest
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("cleaning_date_time")]
        public DateTime CleaningDateTime { get; set; }

        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; } = null!;

        public int ProtocolId { get; set; }

        [ForeignKey(nameof(ProtocolId))]
        public Protocol Protocol { get; set; } = null!;

        [Column("cleaning_type")]
        public CleaningTypes CleaningType { get; set; }

        // Exactly one of these four is non-null, matching CleaningType.
        public int? TargetDepartmentId { get; set; }

        [ForeignKey(nameof(TargetDepartmentId))]
        public Department? TargetDepartment { get; set; }

        public int? ScaleId { get; set; }

        [ForeignKey(nameof(ScaleId))]
        public Scale? Scale { get; set; }

        public int? MachineId { get; set; }

        [ForeignKey(nameof(MachineId))]
        public Machine? Machine { get; set; }

        public int? ClotheId { get; set; }

        [ForeignKey(nameof(ClotheId))]
        public Clothe? Clothe { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}