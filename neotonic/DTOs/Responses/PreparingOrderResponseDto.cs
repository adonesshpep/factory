namespace neotonic.DTOs.Responses
{
    public class PreparingOrderResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int ProtocolId { get; set; }
        public string ProtocolNumber { get; set; } = string.Empty;
        public string ProtocolNameEn { get; set; } = string.Empty;
        public string ProtocolNameAr { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TransferOrderResponseDto? TransferOrder { get; set; }
    }
}