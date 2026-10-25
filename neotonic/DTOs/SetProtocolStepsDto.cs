using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class SetProtocolStepsDto
    {
        [Required]
        public List<int> StepIds { get; set; } = new();

    }
}
