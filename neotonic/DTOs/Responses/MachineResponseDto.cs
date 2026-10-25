namespace neotonic.DTOs.Responses
{
    public class MachineResponseDto
    {
        public int Id { get; set; }
        public string MachineNumber { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentNameEn { get; set; } = string.Empty;
        public string DepartmentNameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
