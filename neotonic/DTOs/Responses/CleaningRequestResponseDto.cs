using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class CleaningRequestResponseDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime CleaningDateTime { get; set; }

        public int DepartmentId { get; set; }
        public string DepartmentNameEn { get; set; } = string.Empty;
        public string DepartmentNameAr { get; set; } = string.Empty;

        public int ProtocolId { get; set; }
        public string ProtocolNumber { get; set; } = string.Empty;
        public string ProtocolNameEn { get; set; } = string.Empty;
        public string ProtocolNameAr { get; set; } = string.Empty;

        public CleaningTypes CleaningType { get; set; }
        public int RelatedId { get; set; }
        public string RelatedNameEn { get; set; } = string.Empty;
        public string RelatedNameAr { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}