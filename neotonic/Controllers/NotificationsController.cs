using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Responses;
using neotonic.Models;

namespace neotonic.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context) => _context = context;

        [HttpGet("index")]
        public async Task<IActionResult> Index(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 20,
     [FromQuery] bool unreadOnly = false)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            if (userId is null) return Unauthorized();

            var visible = _context.Notifications
                .Where(n => n.UserId == userId || (n.UserId == null && roles.Contains(n.Role)));

            var unreadCount = await visible.CountAsync(n => !n.IsRead);

            var query = unreadOnly ? visible.Where(n => !n.IsRead) : visible;

            var ordered = query.OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id);
            var totalCount = await ordered.CountAsync();
            var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Role = n.Role,
                    UserId = n.UserId,
                    Message = n.Message,
                    RelatedType = n.RelatedType,
                    RelatedId = n.RelatedId,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt
                }).ToListAsync();

            return Ok(new
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                UnreadCount = unreadCount
            });
        }
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            if (userId is null) return Unauthorized();

            var count = await _context.Notifications
                .Where(n => !n.IsRead
                            && (n.UserId == userId || (n.UserId == null && roles.Contains(n.Role))))
                .CountAsync();

            return Ok(new { unreadCount = count });
        }
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            if (userId is null) return Unauthorized();

            var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id);
            if (n is null) return NotFound(new { message = "Notification not found" });

            var visible = (n.UserId != null && n.UserId == userId)
                       || (n.UserId == null && roles.Contains(n.Role));
            if (!visible) return Forbid();

            if (!n.IsRead)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            if (userId is null) return Unauthorized();

            var unread = await _context.Notifications
                .Where(n => !n.IsRead
                            && (n.UserId == userId || (n.UserId == null && roles.Contains(n.Role))))
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }
            await _context.SaveChangesAsync();

            return Ok(new { markedRead = unread.Count });
        }
    }
}