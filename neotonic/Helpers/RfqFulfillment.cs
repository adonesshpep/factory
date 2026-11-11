using Microsoft.EntityFrameworkCore;
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class RfqFulfillment
    {
        // Sum of CR.Quantity per RFQ.
        public static async Task<Dictionary<int, decimal>> GetContractedQuantitiesAsync(
            this AppDbContext context, IEnumerable<int> rfqIds, CancellationToken ct = default)
        {
            var ids = rfqIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            return await context.ContractReviews
                .Where(c => c.IsActive && ids.Contains(c.RFQId))
                .GroupBy(c => c.RFQId)
                .Select(g => new { RfqId = g.Key, Total = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.RfqId, x => x.Total, ct);
        }

        // Sum of non-cancelled, active POs per RFQ (through CR).
        public static async Task<Dictionary<int, decimal>> GetOrderedQuantitiesAsync(
            this AppDbContext context, IEnumerable<int> rfqIds, CancellationToken ct = default)
        {
            var ids = rfqIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            return await context.PurchaseOrders
                .Where(p => p.IsActive
                            && p.Status != POStatus.Cancelled
                            && p.ContractReview.IsActive
                            && ids.Contains(p.ContractReview.RFQId))
                .GroupBy(p => p.ContractReview.RFQId)
                .Select(g => new { RfqId = g.Key, Total = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.RfqId, x => x.Total, ct);
        }

        public static decimal Percentage(decimal fulfilled, decimal total) =>
            total <= 0 ? 0 : Math.Round(fulfilled / total * 100m, 2);
    }
}