// Models/AuditLogRole.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace neotonic.Models
{
    [Index(nameof(RoleName))] // non-unique, matches original HasIndex without IsUnique
    public class AuditLogRole
    {
        [Key]
        public int Id { get; set; }

        public int AuditLogId { get; set; }

        [ForeignKey(nameof(AuditLogId))]
        public AuditLog AuditLog { get; set; } = null!;

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}