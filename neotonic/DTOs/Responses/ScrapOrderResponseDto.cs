namespace neotonic.DTOs.Responses
{
    public class ScrapOrderResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public TransferOrderResponseDto? TransferOrder { get; set; }
    }
}