using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class ChangeStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
