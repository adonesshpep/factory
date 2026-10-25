using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateCleaningRequestDto
    {
        [MaxLength(50)] public string? Number { get; set; }
        public DateTime? CleaningDateTime { get; set; }

        [Range(1, int.MaxValue)] public int? DepartmentId { get; set; }
        [Range(1, int.MaxValue)] public int? ProtocolId { get; set; }

        // Must be sent together, or both omitted.
        [EnumDataType(typeof(CleaningTypes))] public CleaningTypes? CleaningType { get; set; }
        [Range(1, int.MaxValue)] public int? RelatedId { get; set; }
    }
}
