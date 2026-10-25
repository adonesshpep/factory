using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Responses;
namespace neotonic.Controllers
{
        [Route("api/audit-logs")]
        [ApiController]
        [Authorize(Roles = "Admin")]
        [Authorize(Policy = "ActiveUser")]
    public class AuditLogsController : ControllerBase
        {
            private readonly AppDbContext _context;

            public AuditLogsController(AppDbContext context)
            {
                _context = context;
            }
        [HttpGet("index")]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20; 

            var query = _context.AuditLogs
                .Include(l => l.Roles)
                .OrderByDescending(l => l.Timestamp);

            var totalCount = await query.CountAsync();

            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AuditLogResponseDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserEmail = l.UserEmail,
                    Roles = l.Roles.Select(r => r.RoleName).ToList(),
                    Action = l.Action,
                    Details = l.Details,
                    IpAddress = l.IpAddress,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            var result = new PagedResultDto<AuditLogResponseDto>
            {
                Items = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

    }
    }
