using neotonic.Models;

namespace neotonic.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context) => _context = context;

        public Task NotifyAsync(string role, string message,
                                NotificationRelatedTypes relatedType, int? relatedId = null,
                                string? userId = null, CancellationToken ct = default)
        {
            _context.Notifications.Add(new Notification
            {
                Role = role,
                UserId = userId,
                Message = message.Length > 1000 ? message[..1000] : message,
                RelatedType = relatedType,
                RelatedId = relatedId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                ReadAt = null
            });
            return Task.CompletedTask;
        }
    }
}