namespace neotonic.DTOs.Responses
{
    public class WorkerResponseDto
    {
        public int Id { get; set; }
        public int WorkerNumber { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public bool IsActive { get; set; }
    }
}