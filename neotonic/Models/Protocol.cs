// Models/Protocol.cs
// FLAGGED: StepTypeId/StepType below is copied exactly as you shared it, but
// this almost certainly should be ProtocolTypeId/ProtocolType. Left as-is per
// your instruction to just convert — tell me if I should fix it.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("protocols")]
    [Index(nameof(ProtocolNumber), IsUnique = true, Name = "ix_protocols_protocol_number")]
    [Index(nameof(NameEn), IsUnique = true, Name = "ix_protocols_name_en")]
    [Index(nameof(NameAr), IsUnique = true, Name = "ix_protocols_name_ar")]
    public class Protocol
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("protocol_number")]
        public string ProtocolNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public int StepTypeId { get; set; }

        [ForeignKey(nameof(StepTypeId))]
        public StepType StepType { get; set; } = null!;

        public ICollection<ProtocolStep> ProtocolSteps { get; set; } = new List<ProtocolStep>();
    }
}