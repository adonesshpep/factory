using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateShelfDto
    {
        [Required, Range(1, int.MaxValue)]
        public int LocationId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Number { get; set; }
    }
}