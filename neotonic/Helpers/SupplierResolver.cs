using Microsoft.EntityFrameworkCore;
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class SupplierResolver
    {
        // Returns the supplier id a box belongs to, or null if it can't be determined.
        // Order of resolution:
        //   1. Latest active delivery sample on the box -> ContractReview.SupplierId
        //   2. MemoRWDetail -> MemoRWIn -> PurchaseOrder -> ContractReview.SupplierId
        //   3. Parent box (recursively) for split children
        public static async Task<int?> GetSupplierIdAsync(
            this AppDbContext context, int uomId, CancellationToken ct = default)
        {
            var visited = new HashSet<int>();
            var current = (int?)uomId;

            while (current is not null && visited.Add(current.Value))
            {
                var id = current.Value;

                // Path 1: delivery sample on this box
                var sampleSupplierId = await context.Samples
                    .Where(s => s.UomId == id && s.IsActive && s.SampleType == SampleTypes.Delivery)
                    .OrderByDescending(s => s.SampleDate).ThenByDescending(s => s.Id)
                    .Select(s => (int?)s.ContractReview!.SupplierId)
                    .FirstOrDefaultAsync(ct);
                if (sampleSupplierId is not null) return sampleSupplierId;

                // Path 2: memo chain
                var memoSupplierId = await context.MemoRWDetails
                    .Where(d => d.UomId == id)
                    .Select(d => (int?)d.Memo.PurchaseOrder.ContractReview.SupplierId)
                    .FirstOrDefaultAsync(ct);
                if (memoSupplierId is not null) return memoSupplierId;

                // Path 3: walk parent
                current = await context.UOMs
                    .Where(u => u.Id == id)
                    .Select(u => u.ParentUomId)
                    .FirstOrDefaultAsync(ct);
            }

            return null;
        }

        // Batch version — one round trip per path per id set. Cheaper when you have many boxes.
        public static async Task<Dictionary<int, int?>> GetSupplierIdsAsync(
            this AppDbContext context, IEnumerable<int> uomIds, CancellationToken ct = default)
        {
            var ids = uomIds.Distinct().ToList();
            var result = new Dictionary<int, int?>();
            foreach (var id in ids)
                result[id] = await context.GetSupplierIdAsync(id, ct);
            return result;
        }
    }
}