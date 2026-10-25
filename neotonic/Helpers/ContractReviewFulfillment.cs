using Microsoft.EntityFrameworkCore;
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class ContractReviewFulfillment
    {
        // Total PO quantity per contract review. Counts active, non-cancelled POs only.
        // excludePurchaseOrderId lets an edit ignore the PO being edited.
        public static async Task<Dictionary<int, decimal>> GetFulfilledQuantitiesAsync(
            this AppDbContext context,
            IEnumerable<int> contractReviewIds,
            int? excludePurchaseOrderId = null)
        {
            var ids = contractReviewIds.Distinct().ToList();

            return await context.PurchaseOrders
                .Where(p => ids.Contains(p.ContractReviewId)
                            && p.IsActive
                            && p.Status != POStatus.Cancelled
                            && (excludePurchaseOrderId == null || p.Id != excludePurchaseOrderId))
                .GroupBy(p => p.ContractReviewId)
                .Select(g => new { ContractReviewId = g.Key, Total = g.Sum(p => p.Quantity) })
                .ToDictionaryAsync(x => x.ContractReviewId, x => x.Total);
        }

        public static decimal Percentage(decimal fulfilled, decimal total) =>
            total <= 0 ? 0 : Math.Round(fulfilled / total * 100, 2);
    }
}