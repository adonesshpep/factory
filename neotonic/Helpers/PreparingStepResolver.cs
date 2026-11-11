using Microsoft.EntityFrameworkCore;
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class PreparingStepResolver
    {
        public static async Task<List<Location>> GetProtocolLocationsAsync(
            this AppDbContext context, int protocolId, CancellationToken ct = default)
        {
            var steps = await context.ProtocolSteps
                .Where(ps => ps.ProtocolId == protocolId)
                .OrderBy(ps => ps.OrderIndex)
                .Include(ps => ps.Step).ThenInclude(s => s.VirtualLocation)
                .ToListAsync(ct);

            return steps
                .Where(ps => ps.Step.VirtualLocationId.HasValue)
                .Select(ps => ps.Step.VirtualLocation!)
                .ToList();
        }

        public static int GetCurrentStepIndex(List<Location> protocolLocations, int currentLocationId) =>
            protocolLocations.FindIndex(l => l.Id == currentLocationId);

        public static async Task<int> GetNextDestinationAsync(
            this AppDbContext context, List<Location> protocolLocations, int currentLocationId,
            CancellationToken ct = default)
        {
            var idx = GetCurrentStepIndex(protocolLocations, currentLocationId);
            if (idx < 0)
                throw new InvalidOperationException("Boxes are not in a location belonging to this protocol");

            if (idx == protocolLocations.Count - 1)
            {
                var i2Acc = await context.Locations
                    .FirstOrDefaultAsync(l => l.InventoryId == InventoryIds.Preparing && l.Zone == InventoryZones.Accepted, ct)
                    ?? throw new InvalidOperationException("I2 accepted zone not seeded");
                return i2Acc.Id;
            }

            return protocolLocations[idx + 1].Id;
        }
    }
}