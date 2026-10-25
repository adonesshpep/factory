namespace neotonic.DTOs.Responses
{
    public class AuditLogResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public List<string> Roles { get; set; } = new();
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
