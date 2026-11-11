using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationRelatedTypes RelatedType { get; set; }
        public int? RelatedId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}