using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateDepartmentDto
    {
        [Required, MaxLength(50)]
        public string DepartmentNumber { get; set; } = string.Empty;

        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }
    }
}
