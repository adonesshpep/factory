using Microsoft.AspNetCore.Authorization;
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
    [Route("api/cleaningrequests")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class CleaningRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public CleaningRequestsController(AppDbContext context, IAuditLogService auditLogService, IConfiguration config)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateCleaningRequestDto dto)
        {
            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department is null || !department.IsActive)
            {
                return BadRequest(new { message = "Department does not exist or is inactive" });
            }

            var protocolError = await ValidateCleaningProtocolAsync(dto.ProtocolId);
            if (protocolError is not null)
            {
                return BadRequest(new { message = protocolError });
            }

            var request = new CleaningRequest
            {
                Number = dto.Number.Trim(),
                CleaningDateTime = dto.CleaningDateTime!.Value,
                DepartmentId = department.Id,
                ProtocolId = dto.ProtocolId
            };

            var targetError = await ApplyTargetAsync(request, dto.CleaningType!.Value, dto.RelatedId!.Value);
            if (targetError is not null)
            {
                return BadRequest(new { message = targetError });
            }

            try
            {
                _context.CleaningRequests.Add(request);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A cleaning request with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateCleaningRequest",
                $"Created cleaning request '{request.Number}' (Id: {request.Id}, Type: {request.CleaningType})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(request.Id));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateCleaningRequestDto dto)
        {
            var request = await _context.CleaningRequests.FindAsync(id);
            if (request is null)
            {
                return NotFound(new { message = "Cleaning request not found" });
            }

            if (dto.CleaningType.HasValue != dto.RelatedId.HasValue)
            {
                return BadRequest(new { message = "CleaningType and RelatedId must be sent together" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Number))
                request.Number = dto.Number.Trim();

            if (dto.CleaningDateTime.HasValue)
                request.CleaningDateTime = dto.CleaningDateTime.Value;

            if (dto.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(dto.DepartmentId.Value);
                if (department is null || !department.IsActive)
                    return BadRequest(new { message = "Department does not exist or is inactive" });
                request.DepartmentId = department.Id;
            }

            if (dto.ProtocolId.HasValue)
            {
                var protocolError = await ValidateCleaningProtocolAsync(dto.ProtocolId.Value);
                if (protocolError is not null)
                    return BadRequest(new { message = protocolError });
                request.ProtocolId = dto.ProtocolId.Value;
            }

            if (dto.CleaningType.HasValue)
            {
                var targetError = await ApplyTargetAsync(request, dto.CleaningType.Value, dto.RelatedId!.Value);
                if (targetError is not null)
                    return BadRequest(new { message = targetError });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A cleaning request with this number already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "UpdateCleaningRequest",
                $"Updated cleaning request (Id: {request.Id}, Number: {request.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(request.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = QueryWithIncludes()
                .OrderByDescending(r => r.CleaningDateTime)
                .ThenByDescending(r => r.Id);

            var totalCount = await query.CountAsync();

            var requests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResultDto<CleaningRequestResponseDto>
            {
                Items = requests.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var request = await _context.CleaningRequests.FindAsync(id);
            if (request is null)
            {
                return NotFound(new { message = "Cleaning request not found" });
            }

            request.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateCleaningRequest" : "DeactivateCleaningRequest",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} cleaning request '{request.Number}' (Id: {request.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Cleaning request {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.CleaningRequests.FindAsync(id);
            if (request is null)
            {
                return NotFound(new { message = "Cleaning request not found" });
            }

            _context.CleaningRequests.Remove(request);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(request);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteCleaningRequest",
                $"Deleted cleaning request '{request.Number}' (Id: {request.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Cleaning request deleted successfully" });
        }

        // ---- helpers ----

        private async Task<string?> ValidateCleaningProtocolAsync(int protocolId)
        {
            var protocol = await _context.Protocols
                .Include(p => p.StepType)
                .FirstOrDefaultAsync(p => p.Id == protocolId);

            if (protocol is null || !protocol.IsActive)
                return "Protocol does not exist or is inactive";

            if (!CleaningStepType.Matches(protocol.StepType))
                return "Protocol is not a cleaning protocol";

            return null;
        }

        // Maps (type, relatedId) onto the one matching FK column and clears the other three.
        private async Task<string?> ApplyTargetAsync(CleaningRequest request, CleaningTypes type, int relatedId)
        {
            request.CleaningType = type;
            request.TargetDepartmentId = null;
            request.ScaleId = null;
            request.MachineId = null;
            request.ClotheId = null;

            switch (type)
            {
                case CleaningTypes.Department:
                    {
                        var target = await _context.Departments.FindAsync(relatedId);
                        if (target is null || !target.IsActive)
                            return "Related department does not exist or is inactive";
                        request.TargetDepartmentId = target.Id;
                        return null;
                    }
                case CleaningTypes.Scale:
                    {
                        var target = await _context.Scales.FindAsync(relatedId);
                        if (target is null || !target.IsActive)
                            return "Related scale does not exist or is inactive";
                        request.ScaleId = target.Id;
                        return null;
                    }
                case CleaningTypes.Machine:
                    {
                        var target = await _context.Machines.FindAsync(relatedId);
                        if (target is null || !target.IsActive)
                            return "Related machine does not exist or is inactive";
                        request.MachineId = target.Id;
                        return null;
                    }
                case CleaningTypes.Clothe:
                    {
                        var target = await _context.Clothes.FindAsync(relatedId);
                        if (target is null || !target.IsActive)
                            return "Related clothe does not exist or is inactive";
                        request.ClotheId = target.Id;
                        return null;
                    }
                default:
                    return "Invalid cleaning type";
            }
        }

        private IQueryable<CleaningRequest> QueryWithIncludes() =>
            _context.CleaningRequests
                .Include(r => r.Department)
                .Include(r => r.Protocol)
                .Include(r => r.TargetDepartment)
                .Include(r => r.Scale)
                .Include(r => r.Machine)
                .Include(r => r.Clothe);

        private async Task<CleaningRequestResponseDto> ToResponseDto(int id)
        {
            var request = await QueryWithIncludes().FirstAsync(r => r.Id == id);
            return ToDto(request);
        }

        private static CleaningRequestResponseDto ToDto(CleaningRequest r)
        {
            var (relatedId, relatedEn, relatedAr) = r.CleaningType switch
            {
                CleaningTypes.Department => (r.TargetDepartmentId, r.TargetDepartment?.NameEn, r.TargetDepartment?.NameAr),
                CleaningTypes.Scale => (r.ScaleId, r.Scale?.NameEn, r.Scale?.NameAr),
                CleaningTypes.Machine => (r.MachineId, r.Machine?.NameEn, r.Machine?.NameAr),
                CleaningTypes.Clothe => (r.ClotheId, r.Clothe?.NameEn, r.Clothe?.NameAr),
                _ => ((int?)null, (string?)null, (string?)null)
            };

            return new CleaningRequestResponseDto
            {
                Id = r.Id,
                Number = r.Number,
                CleaningDateTime = r.CleaningDateTime,
                DepartmentId = r.DepartmentId,
                DepartmentNameEn = r.Department.NameEn,
                DepartmentNameAr = r.Department.NameAr,
                ProtocolId = r.ProtocolId,
                ProtocolNumber = r.Protocol.ProtocolNumber,
                ProtocolNameEn = r.Protocol.NameEn,
                ProtocolNameAr = r.Protocol.NameAr,
                CleaningType = r.CleaningType,
                RelatedId = relatedId ?? 0,
                RelatedNameEn = relatedEn ?? string.Empty,
                RelatedNameAr = relatedAr ?? string.Empty,
                IsActive = r.IsActive
            };
        }
    }
}