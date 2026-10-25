using neotonic.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace neotonic.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }
        public async Task LogAsync(ClaimsPrincipal user, string action, string? details = null, string? ipAddress = null)
        {
            var roleNames = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var log = new AuditLog
            {
                UserId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "unknown",
                UserEmail = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                Roles = roleNames.Select(r => new AuditLogRole { RoleName = r }).ToList(),
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        // AuditLogService.cs
        public async Task LogAsync(string userId, string? userEmail, IEnumerable<string> roles, string action, string? details = null, string? ipAddress = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Roles = roles.Select(r => new AuditLogRole { RoleName = r }).ToList()
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
