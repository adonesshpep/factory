namespace neotonic.DTOs
{
    public class MemoRWInResponseDto
    {
        public int Id { get; set; }
        public long Number { get; set; }
        public DateTime MemoDate { get; set; }

        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;

        public int WorkerId { get; set; }
        public int WorkerNumber { get; set; }
        public string WorkerNameEn { get; set; } = string.Empty;
        public string WorkerNameAr { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public List<MemoRWDetailResponseDto> Details { get; set; } = new();
    }
}