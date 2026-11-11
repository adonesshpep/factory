using neotonic.Models;

namespace neotonic.Services
{
    public interface INotificationService
    {
        // Adds to the shared DbContext. Does not SaveChanges — the caller's transaction commits.
        Task NotifyAsync(string role, string message,
                         NotificationRelatedTypes relatedType, int? relatedId = null,
                         string? userId = null, CancellationToken ct = default);
    }
}