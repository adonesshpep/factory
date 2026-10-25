namespace neotonic.DTOs.Responses
{
    public class DepartmentResponseDto
    {
        public int Id { get; set; }
        public string DepartmentNumber { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
