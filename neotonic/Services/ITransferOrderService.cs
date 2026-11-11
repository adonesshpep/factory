using neotonic.DTOs.Responses;
using neotonic.Models;

namespace neotonic.Services
{
    public interface ITransferOrderService
    {
        Task<TransferOrder> CreateAsync(int fromLocationId, int toLocationId, int materialId,
                                        decimal quantity, bool allowOffFlowDestination = false,
                                        CancellationToken ct = default);

        Task<TransferOrder> SetLinesAsync(int transferOrderId,
                                          IEnumerable<(int UomId, decimal Quantity)> lines,
                                          CancellationToken ct = default);

        Task<TransferOrder> MarkDoneAsync(int transferOrderId, CancellationToken ct = default);

        Task<TransferOrder> CancelAsync(int transferOrderId, CancellationToken ct = default);

        Task<TransferOrder> FinishAsync(int transferOrderId, IEnumerable<int> scannedUomIds,
                                        CancellationToken ct = default);

        Task<List<UomListItemDto>> GetCandidatesAsync(int transferOrderId, CancellationToken ct = default);

        Task<decimal> GetReservedAsync(int uomId, CancellationToken ct = default);
        Task<decimal> GetAvailableAsync(int uomId, CancellationToken ct = default);
    }
}