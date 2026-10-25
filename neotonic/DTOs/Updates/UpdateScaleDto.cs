namespace neotonic.DTOs.Updates
{
    public class UpdateScaleDto
    {
        public string? ScaleNumber { get; set; }
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }
        public string? Notes { get; set; }

        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? DepartmentId { get; set; }
    }
}
