using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateWorkerDto
    {
        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }
        [MaxLength(30)] public string? PhoneNumber { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}