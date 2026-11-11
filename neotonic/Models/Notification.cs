using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("notifications")]
    [Index(nameof(Role), nameof(IsRead), Name = "ix_notifications_role_read")]
    [Index(nameof(UserId), nameof(IsRead), Name = "ix_notifications_user_read")]
    [Index(nameof(CreatedAt), Name = "ix_notifications_created")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [MaxLength(450)]
        [Column("user_id")]
        public string? UserId { get; set; }

        [Required, MaxLength(1000)]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("related_type")]
        public NotificationRelatedTypes RelatedType { get; set; }

        [Column("related_id")]
        public int? RelatedId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_read")]
        public bool IsRead { get; set; }

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }
    }
}