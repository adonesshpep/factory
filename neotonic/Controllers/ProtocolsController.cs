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
using System.Linq.Expressions;

namespace neotonic.Controllers
{
    [Route("api/protocols")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ProtocolsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public ProtocolsController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateProtocolDto dto)
        {
            var nameEn = dto.NameEn?.Trim();
            var nameAr = dto.NameAr?.Trim();

            if (!BilingualFieldResolver.TryResolve(nameEn, nameAr, out var resolvedEn, out var resolvedAr))
            {
                return BadRequest(new { message = "At least one of English Name or Arabic Name must be provided" });
            }
            var StepType = await _context.StepTypes.FindAsync(dto.StepTypeId);
            if (StepType is null || !StepType.IsActive)
            {
                return NotFound(new { message = "Protocol Type does not exist or is inactive" });
            }
            var protocol = new Protocol
            {
                ProtocolNumber = dto.ProtocolNumber.Trim(),
                NameEn = resolvedEn,
                NameAr = resolvedAr,
                StepTypeId=StepType.Id
            };

            try
            {
                _context.Protocols.Add(protocol);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = $"A  protocol with this {FieldFromIndex(sqlEx.Message)} already exists" });
            }

            await _auditLogService.LogAsync(
                User,
                "CreateProtocol",
                $"Created protocol '{protocol.NameEn}' / '{protocol.NameAr}' (Id: {protocol.Id}, Number: {protocol.ProtocolNumber} Type {protocol.StepTypeId})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new ProtocolResponseDto
            {
                Id = protocol.Id,
                ProtocolNumber = protocol.ProtocolNumber,
                NameEn = protocol.NameEn,
                NameAr = protocol.NameAr,
                StepTypeId=protocol.StepTypeId,
                StepTypeNameAr=protocol.StepType.NameAr,
                StepTypeNameEn=protocol.StepType.NameEn,
                IsActive = protocol.IsActive,
                Steps = new List<ProtocolStepResponseDto>()
            });
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateProtocolDto dto)
        {
            var protocol = await _context.Protocols.FindAsync(id);
            if (protocol is null)
            {
                return NotFound(new { message = "Protocol not found" });
            }

            if (!string.IsNullOrWhiteSpace(dto.ProtocolNumber))
                protocol.ProtocolNumber = dto.ProtocolNumber.Trim();

            var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
            var nameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? null : dto.NameAr.Trim();
            if (nameEn != null) protocol.NameEn = nameEn;
            if (nameAr != null) protocol.NameAr = nameAr;
            if (dto.StepTypeId.HasValue)
            {
                var StepType = await _context.StepTypes.FindAsync(dto.StepTypeId);
                if (StepType is null || !StepType.IsActive)
                {
                    return NotFound(new { message = "Protocol Type does not exist or is inactive" });
                }
                protocol.StepTypeId = StepType.Id;
            }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
                {
                    return Conflict(new { message = $"A protocol with this {FieldFromIndex(sqlEx.Message)} already exists" });
                }

            await _auditLogService.LogAsync(
                User,
                "UpdateProtocol",
                $"Updated protocol (Id: {protocol.Id}, Number: {protocol.ProtocolNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(protocol));
        }
        private static string FieldFromIndex(string sqlMessage) =>
    sqlMessage.Contains("ix_protocols_protocol_number") ? "protocol number" :
    sqlMessage.Contains("ix_protocols_name_en") ? "English name" :
    sqlMessage.Contains("ix_protocols_name_ar") ? "Arabic name" :
    "value";

    
   


        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Protocols.OrderBy(p => p.Id);
            var totalCount = await query.CountAsync();

            var protocols = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProtocolListItemDto
                {
                    Id = p.Id,
                    ProtocolNumber = p.ProtocolNumber,
                    NameEn = p.NameEn,
                    NameAr = p.NameAr,
                    StepTypeId = p.StepTypeId,
                    StepTypeNameAr=p.StepType.NameAr,
                    StepTypeNameEn=p.StepType.NameEn,
                    IsActive = p.IsActive,
                    StepCount = p.ProtocolSteps.Count
                })
                .ToListAsync();

            return Ok(new PagedResultDto<ProtocolListItemDto>
            {
                Items = protocols,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var protocol = await _context.Protocols.FindAsync(id);
            if (protocol is null)
            {
                return NotFound(new { message = "Protocol not found" });
            }

            return Ok(await ToResponseDto(protocol));
        }
        [HttpPut("{id}/steps")]
        public async Task<IActionResult> SetSteps(int id, SetProtocolStepsDto dto)
        {
            var protocol = await _context.Protocols.FindAsync(id);
            if (protocol is null)
            {
                return NotFound(new { message = "Protocol not found" });
            }

            //if (dto.StepIds.Distinct().Count() != dto.StepIds.Count)
            //{
            //    return BadRequest(new { message = "The same step cannot appear twice in one protocol" });
            //}

            var steps = new List<Step>();
            foreach (var stepId in dto.StepIds)
            {
                var step = await _context.Steps.FindAsync(stepId);
                if (step is null || !step.IsActive)
                {
                    return BadRequest(new { message = $"Step {stepId} does not exist or is inactive" });
                }
                steps.Add(step);
            }

            var existing = await _context.ProtocolSteps.Where(ps => ps.ProtocolId == id).ToListAsync();
            _context.ProtocolSteps.RemoveRange(existing);

            for (int i = 0; i < steps.Count; i++)
            {
                _context.ProtocolSteps.Add(new ProtocolStep
                {
                    ProtocolId = id,
                    StepId = steps[i].Id,
                    OrderIndex = i + 1
                });
            }

            // Both the removal and the inserts are tracked on the same context
            // and flushed together, so this is one atomic save, not two steps
            // that could leave the protocol half-updated if something fails partway.
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                "SetProtocolSteps",
                $"Set step order for protocol (Id: {id}) to [{string.Join(", ", dto.StepIds)}]",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(protocol));
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var protocol = await _context.Protocols.FindAsync(id);
            if (protocol is null)
            {
                return NotFound(new { message = "Protocol not found" });
            }

            protocol.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateProtocol" : "DeactivateProtocol",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} protocol '{protocol.NameEn}' / '{protocol.NameAr}' (Id: {protocol.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Protocol {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var protocol = await _context.Protocols.FindAsync(id);
            if (protocol is null)
            {
                return NotFound(new { message = "Protocol not found" });
            }

            _context.Protocols.Remove(protocol);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(protocol);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User,
                "DeleteProtocol",
                $"Deleted protocol '{protocol.NameEn}' / '{protocol.NameAr}' (Id: {protocol.Id}, Number: {protocol.ProtocolNumber})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "protocol deleted successfully" });
        }

        private async Task<ProtocolResponseDto> ToResponseDto(Protocol protocol)
        {
            await _context.Entry(protocol).Reference(s => s.StepType).LoadAsync();
            var steps = await _context.ProtocolSteps
                .Where(ps => ps.ProtocolId == protocol.Id)
                .Include(ps => ps.Step)
                .OrderBy(ps => ps.OrderIndex)
                .Select(ps => new ProtocolStepResponseDto
                {
                    OrderIndex = ps.OrderIndex,
                    StepId = ps.StepId,
                    StepNumber = ps.Step.StepNumber,
                    StepNameEn = ps.Step.StepNameEn,
                    StepNameAr = ps.Step.StepNameAr
                })
                .ToListAsync();

            return new ProtocolResponseDto
            {
                Id = protocol.Id,
                ProtocolNumber = protocol.ProtocolNumber,
                NameEn = protocol.NameEn,
                NameAr = protocol.NameAr,
                StepTypeId=protocol.StepTypeId,
                StepTypeNameEn = protocol.StepType?.NameEn,
                StepTypeNameAr=protocol.StepType?.NameAr,
                IsActive = protocol.IsActive,
                Steps = steps
            };
        }
    }
    }

