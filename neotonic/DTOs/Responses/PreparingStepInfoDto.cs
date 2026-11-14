namespace neotonic.DTOs.Responses
{
    public class PreparingStepInfoDto
    {
        public int OrderIndex { get; set; }
        public int VirtualLocationId { get; set; }
        public string VirtualLocationCode { get; set; } = string.Empty;
        public string VirtualLocationNameEn { get; set; } = string.Empty;
        public string VirtualLocationNameAr { get; set; } = string.Empty;
    }
}