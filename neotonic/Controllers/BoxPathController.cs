using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Helpers;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/boxpath")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    [Authorize]
    public class BoxPathController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        public BoxPathController(AppDbContext context, IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
            _context = context;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateBoxPathDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                return BadRequest(new { message = "Name and Code are both required" });
            }

            var boxpath = new BoxPath
            {
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim()
            };
            try
            {
                _context.BoxPaths.Add(boxpath);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A box path with this code already exists" });
            }
            await _auditLogService.LogAsync(
    User,
    "CreateBoxPath",
    $"Created box path '{boxpath.Name}' / '{boxpath.Code}' (Id: {boxpath.Id})",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(boxpath);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(UpdateBoxPathDto dto, int id)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) dto.Name = null;
            if (string.IsNullOrWhiteSpace(dto.Code)) dto.Code = null;
            if (dto.Name == null && dto.Code == null)
            {
                return BadRequest(new { message = "At least one of Name or Code must be provided" });
            }
            var boxpath = await _context.BoxPaths.FindAsync(id);
            if (boxpath == null)
            {
                return NotFound(new { message = "Box Path not found" });
            }
            var oldName = boxpath.Name;
            var oldCode = boxpath.Code;
            if (dto.Name != null)
            {
                boxpath.Name = dto.Name;
            }
            if (dto.Code != null)
            {
                boxpath.Code = dto.Code;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A box path with this code already exists" });
            }
            await _auditLogService.LogAsync(
    User,
    "UpdateBoxPath",
    $"Updated box path from '{oldName}' to '{boxpath.Name}' / and from '{oldCode}' to '{boxpath.Code}' (Id: {boxpath.Id})",
    HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(boxpath);
        }
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.BoxPaths.OrderBy(c => c.Id);

            var totalCount = await query.CountAsync();

            var boxpaths = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new BoxPathResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    IsActive = c.IsActive,
                })
                .ToListAsync();

            var result = new PagedResultDto<BoxPathResponseDto>
            {
                Items = boxpaths,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            
            var boxPath = await _context.BoxPaths.FindAsync(id);
            if (boxPath == null)
            {
                return NotFound(new { message = "Box Path not found" });
            }
            _context.BoxPaths.Remove(boxPath);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Simply call the helper method here!
                var conflictPayload = await _context.GetDeleteConflictsAsync(boxPath);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteBoxPath",
                $"Deleted box path '{boxPath.Name}' / '{boxPath.Code}' (Id: {boxPath.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Box Path deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var boxPath = await _context.BoxPaths.FindAsync(id);
            if (boxPath == null)
            {
                return NotFound(new { message = "Box Path not found" });
            }

            boxPath.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(
     User,
     dto.IsActive ? "ActivateBoxPath" : "DeactivateBoxPath",
     $"{(dto.IsActive ? "Activated" : "Deactivated")} boxPath '{boxPath.Name}' / '{boxPath.Code}' (Id: {boxPath.Id})",
     HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"BoxPath {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
    }
}
