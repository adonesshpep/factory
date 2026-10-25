using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Optional: let the admin assign a role at creation time instead of always defaulting to "User"
        public List<string>? Roles { get; set; }
    }
}
