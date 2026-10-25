using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateCleaningRequestDto
    {
        [Required, MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public DateTime? CleaningDateTime { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int DepartmentId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ProtocolId { get; set; }

        [Required, EnumDataType(typeof(CleaningTypes))]
        public CleaningTypes? CleaningType { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int? RelatedId { get; set; }
    }
}