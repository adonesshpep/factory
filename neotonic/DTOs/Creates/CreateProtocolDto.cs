using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateProtocolDto
    {
        [Required, MaxLength(50)]
        public string ProtocolNumber { get; set; } = string.Empty;

        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }
        [Required] public int StepTypeId { get; set; }

    }
}
