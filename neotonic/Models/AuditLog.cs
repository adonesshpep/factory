// Models/AuditLog.cs
// Unchanged in shape — this table never used snake_case columns or
// HasColumnName in the original, so no [Column] attributes needed.
using System.ComponentModel.DataAnnotations;

namespace neotonic.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? UserEmail { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ICollection<AuditLogRole> Roles { get; set; } = new List<AuditLogRole>();
    }
}