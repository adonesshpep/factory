using Microsoft.EntityFrameworkCore;
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class AnnualPlanFulfillment
    {
        // Requested per (planId, materialId): sum of RFQ.Quantity for RFQs whose AnnualPlanID = planId.
        public static async Task<Dictionary<(int PlanId, int MaterialId), decimal>> GetRequestedByMaterialAsync(
            this AppDbContext context, IEnumerable<int> planIds, CancellationToken ct = default)
        {
            var ids = planIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            var rows = await context.RFQs
                .Where(r => r.AnnualPlanID.HasValue
                            && ids.Contains(r.AnnualPlanID.Value)
                            && r.IsActive)
                .GroupBy(r => new { r.AnnualPlanID, r.MaterialId })
                .Select(g => new
                {
                    PlanId = g.Key.AnnualPlanID!.Value,
                    g.Key.MaterialId,
                    Total = g.Sum(x => x.Quantity)
                })
                .ToListAsync(ct);

            return rows.ToDictionary(x => (x.PlanId, x.MaterialId), x => x.Total);
        }

        // Contracted per (planId, materialId): sum of CR.Quantity through RFQ → CR.
        public static async Task<Dictionary<(int PlanId, int MaterialId), decimal>> GetContractedByMaterialAsync(
            this AppDbContext context, IEnumerable<int> planIds, CancellationToken ct = default)
        {
            var ids = planIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            var rows = await context.ContractReviews
                .Where(c => c.IsActive && c.RFQ.AnnualPlanID.HasValue
                            && ids.Contains(c.RFQ.AnnualPlanID.Value))
                .GroupBy(c => new { PlanId = c.RFQ.AnnualPlanID!.Value, c.RFQ.MaterialId })
                .Select(g => new
                {
                    g.Key.PlanId,
                    g.Key.MaterialId,
                    Total = g.Sum(x => x.Quantity)
                })
                .ToListAsync(ct);

            return rows.ToDictionary(x => (x.PlanId, x.MaterialId), x => x.Total);
        }

        // Ordered per (planId, materialId): sum of non-cancelled, active POs through CR → RFQ.
        public static async Task<Dictionary<(int PlanId, int MaterialId), decimal>> GetOrderedByMaterialAsync(
            this AppDbContext context, IEnumerable<int> planIds, CancellationToken ct = default)
        {
            var ids = planIds.Distinct().ToList();
            if (ids.Count == 0) return new();

            var rows = await context.PurchaseOrders
                .Where(p => p.IsActive
                            && p.Status != POStatus.Cancelled
                            && p.ContractReview.IsActive
                            && p.ContractReview.RFQ.AnnualPlanID.HasValue
                            && ids.Contains(p.ContractReview.RFQ.AnnualPlanID.Value))
                .GroupBy(p => new
                {
                    PlanId = p.ContractReview.RFQ.AnnualPlanID!.Value,
                    p.ContractReview.RFQ.MaterialId
                })
                .Select(g => new
                {
                    g.Key.PlanId,
                    g.Key.MaterialId,
                    Total = g.Sum(x => x.Quantity)
                })
                .ToListAsync(ct);

            return rows.ToDictionary(x => (x.PlanId, x.MaterialId), x => x.Total);
        }

        // Plan-level percentage = average of each material-line percentage, each capped at 100.
        // (Handoff rule: "plan % = average of line % each capped at 100".)
        public static decimal PlanPercentage(
            IEnumerable<(decimal Planned, decimal Fulfilled)> lines)
        {
            var list = lines.ToList();
            if (list.Count == 0) return 0m;

            var capped = list.Select(l =>
            {
                if (l.Planned <= 0) return 0m;
                var pct = l.Fulfilled / l.Planned * 100m;
                return pct > 100m ? 100m : pct;
            }).ToList();

            return Math.Round(capped.Average(), 2);
        }

        // Line-level percentage (uncapped, so over-fulfilment is visible).
        public static decimal Percentage(decimal fulfilled, decimal planned) =>
            planned <= 0 ? 0 : Math.Round(fulfilled / planned * 100m, 2);
    }
}