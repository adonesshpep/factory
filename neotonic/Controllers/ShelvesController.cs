using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.DTOs.Responses;
using neotonic.DTOs.Updates;
using neotonic.Models;
using neotonic.Services;

namespace neotonic.Controllers
{
    [Route("api/shelves")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class ShelvesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public ShelvesController(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateShelfDto dto)
        {
            var location = await _context.Locations
                .Include(l => l.Inventory)
                .FirstOrDefaultAsync(l => l.Id == dto.LocationId);
            if (location is null || !location.IsActive)
                return BadRequest(new { message = "Location does not exist or is inactive" });
            if (location.Kind != LocationKinds.Inventory)
                return BadRequest(new { message = "Virtual locations have no shelves" });

            var shelf = new Shelf
            {
                LocationId = location.Id,
                Number = dto.Number
            };

            try
            {
                _context.Shelves.Add(shelf);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A shelf with this number already exists at this location" });
            }

            await _auditLogService.LogAsync(
                User, "CreateShelf",
                $"Created shelf {ShelfLabel(location, shelf.Number)} (Id: {shelf.Id}, Location: {location.Code})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(shelf.Id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateShelfDto dto)
        {
            var shelf = await _context.Shelves
                .Include(s => s.Location).ThenInclude(l => l.Inventory)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (shelf is null) return NotFound(new { message = "Shelf not found" });

            if (dto.Number.HasValue)
                shelf.Number = dto.Number.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627)
            {
                return Conflict(new { message = "A shelf with this number already exists at this location" });
            }

            await _auditLogService.LogAsync(
                User, "UpdateShelf",
                $"Updated shelf (Id: {shelf.Id}, Label: {ShelfLabel(shelf.Location, shelf.Number)})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(await ToResponseDto(shelf.Id));
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? locationId = null,
            [FromQuery] bool? unoccupiedOnly = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Shelves
                .Include(s => s.Location).ThenInclude(l => l.Inventory)
                .AsQueryable();

            if (locationId.HasValue)
                query = query.Where(s => s.LocationId == locationId.Value);

            if (unoccupiedOnly == true)
                query = query.Where(s => !_context.UOMs.Any(u => u.ShelfId == s.Id));

            var ordered = query.OrderBy(s => s.LocationId).ThenBy(s => s.Number);
            var totalCount = await ordered.CountAsync();
            var shelves = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var occupiedIds = await GetOccupiedShelfIds(shelves.Select(s => s.Id));

            return Ok(new PagedResultDto<ShelfResponseDto>
            {
                Items = shelves.Select(s => ToDto(s, occupiedIds.Contains(s.Id))).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf is null) return NotFound(new { message = "Shelf not found" });

            shelf.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateShelf" : "DeactivateShelf",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} shelf (Id: {shelf.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Shelf {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf is null) return NotFound(new { message = "Shelf not found" });

            _context.Shelves.Remove(shelf);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var conflictPayload = await _context.GetDeleteConflictsAsync(shelf);
                return Conflict(conflictPayload);
            }

            await _auditLogService.LogAsync(
                User, "DeleteShelf",
                $"Deleted shelf (Id: {shelf.Id}, Location: {shelf.LocationId}, Number: {shelf.Number})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Shelf deleted successfully" });
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private async Task<HashSet<int>> GetOccupiedShelfIds(IEnumerable<int> shelfIds)
        {
            var ids = shelfIds.ToList();
            if (ids.Count == 0) return new();

            var occupied = await _context.UOMs
                .Where(u => u.ShelfId != null && ids.Contains(u.ShelfId!.Value))
                .Select(u => u.ShelfId!.Value)
                .Distinct()
                .ToListAsync();

            return occupied.ToHashSet();
        }

        private async Task<ShelfResponseDto> ToResponseDto(int id)
        {
            var shelf = await _context.Shelves
                .Include(s => s.Location).ThenInclude(l => l.Inventory)
                .FirstAsync(s => s.Id == id);

            var occupied = await _context.UOMs.AnyAsync(u => u.ShelfId == id);
            return ToDto(shelf, occupied);
        }

        private static ShelfResponseDto ToDto(Shelf s, bool isOccupied) => new()
        {
            Id = s.Id,
            LocationId = s.LocationId,
            LocationCode = s.Location.Code,
            InventoryCode = s.Location.Inventory?.Code,
            Zone = s.Location.Zone,
            Number = s.Number,
            Label = ShelfLabel(s.Location, s.Number),
            IsActive = s.IsActive,
            IsOccupied = isOccupied
        };

        public static string ShelfLabel(Location location, int number)
        {
            var zoneLetter = location.Zone switch
            {
                InventoryZones.Quarantine => "A",
                InventoryZones.Accepted => "B",
                InventoryZones.Rejected => "C",
                _ => "?"
            };
            return $"{location.Inventory?.Code ?? location.Code}-{zoneLetter}{number}";
        }
    }
}