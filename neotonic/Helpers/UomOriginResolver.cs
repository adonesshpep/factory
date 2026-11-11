using Microsoft.EntityFrameworkCore;
using neotonic.Models;

namespace neotonic.Helpers
{
    public record UomOrigin(
        int? SupplierId,
        string? SupplierNumber,
        string? SupplierNameEn,
        string? SupplierNameAr,
        int? MemoRWInId,
        long? MemoRWInNumber,
        int MaterialId);

    public static class UomOriginResolver
    {
        public static async Task<UomOrigin?> GetAsync(
            this AppDbContext context, int uomId, CancellationToken ct = default)
        {
            var materialId = await context.UOMs
                .Where(u => u.Id == uomId).Select(u => u.MaterialId)
                .FirstOrDefaultAsync(ct);
            if (materialId is null) return null;

            // Walk up the parent chain once, collecting supplier + memo together.
            int? supplierId = null;
            string? supplierNumber = null, supplierNameEn = null, supplierNameAr = null;
            int? memoId = null;
            long? memoNumber = null;

            var visited = new HashSet<int>();
            int? cur = uomId;

            while (cur is not null && visited.Add(cur.Value))
            {
                var via = await context.MemoRWDetails
                    .Where(d => d.UomId == cur.Value)
                    .Select(d => new
                    {
                        d.MemoId,
                        d.Memo.Number,
                        d.Memo.PurchaseOrder.ContractReview.SupplierId,
                        d.Memo.PurchaseOrder.ContractReview.Supplier.SupplierNumber,
                        d.Memo.PurchaseOrder.ContractReview.Supplier.NameEn,
                        d.Memo.PurchaseOrder.ContractReview.Supplier.NameAr
                    })
                    .FirstOrDefaultAsync(ct);

                if (via is not null)
                {
                    memoId = via.MemoId;
                    memoNumber = via.Number;
                    supplierId = via.SupplierId;
                    supplierNumber = via.SupplierNumber;
                    supplierNameEn = via.NameEn;
                    supplierNameAr = via.NameAr;
                    break;
                }

                cur = await context.UOMs.Where(u => u.Id == cur.Value)
                    .Select(u => u.ParentUomId).FirstOrDefaultAsync(ct);
            }

            return new UomOrigin(supplierId, supplierNumber, supplierNameEn, supplierNameAr,
                                 memoId, memoNumber, materialId.Value);
        }
    }
}