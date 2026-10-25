namespace neotonic.DTOs.Responses
{
    public class LabEquipmentResponseDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string JobEn { get; set; } = string.Empty;
        public string JobAr { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime EqualizingDate { get; set; }
        public bool IsActive { get; set; }
    }
}
