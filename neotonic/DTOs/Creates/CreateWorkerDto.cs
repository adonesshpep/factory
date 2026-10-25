// DTOs/CreateWorkerDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateWorkerDto
    {
        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }

        [Required, MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public DateOnly? BirthDate { get; set; }
    }
}