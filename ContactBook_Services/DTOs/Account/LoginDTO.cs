using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContactBook_Services.DTOs.Account
{
    public class LoginDTO
    {
        [Required]
        [DefaultValue("user@sappro.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }

        [Required]
        [DefaultValue("test@test")]
        [MinLength(6, ErrorMessage = "The new password must be at least 6 characters long")]
        public required string Password { get; set; }
    }
}