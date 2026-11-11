using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs.Responses;
using neotonic.Models;

namespace neotonic.Controllers
{
    [Route("api/locations")]
    [ApiController]
    [Authorize(Policy = "ActiveUser")]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context) => _context = context;

        // entryOnly = true  -> only the first FlowStep of each flow (entry locations)
        // entryOnly = false -> all active locations (real + virtual)
        // kind              -> optional filter by LocationKinds (Inventory=1, Virtual=2)
        [HttpGet("index")]
        public async Task<IActionResult> Index(
            [FromQuery] bool entryOnly = false,
            [FromQuery] LocationKinds? kind = null)
        {
            IQueryable<Location> query = _context.Locations
                .Include(l => l.Inventory)
                .Where(l => l.IsActive)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.Id);

            if (entryOnly)
            {
                var entryIds = await _context.FlowSteps
                    .Where(s => s.OrderIndex == 1)
                    .Select(s => s.LocationId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(l => entryIds.Contains(l.Id));
            }

            if (kind.HasValue)
                query = query.Where(l => l.Kind == kind.Value);

            var locations = await query.ToListAsync();

            return Ok(locations.Select(l => new LocationResponseDto
            {
                Id = l.Id,
                Code = l.Code,
                NameEn = l.NameEn,
                NameAr = l.NameAr,
                Kind = l.Kind,
                InventoryId = l.InventoryId,
                InventoryCode = l.Inventory?.Code,
                Zone = l.Zone
            }).ToList());
        }
    }
}