using System.Security.Claims;

namespace neotonic.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(ClaimsPrincipal user, string action, string? details = null, string? ipAddress = null);
        Task LogAsync(string userId, string? userEmail, IEnumerable<string> roles, string action, string? details = null, string? ipAddress = null);
    }
}
