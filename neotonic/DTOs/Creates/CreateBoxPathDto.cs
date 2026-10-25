using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateBoxPathDto
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required,MaxLength(200)] public string? Name { get; set; }
    }
}
