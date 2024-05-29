using System.ComponentModel.DataAnnotations;

namespace ContactBook_Services.DTOs.Account
{
    public class SetPasswordDTO
    {
        [Required]
        [MinLength(6, ErrorMessage = "The new password must be at least 6 characters long")]
        public required string NewPassword { get; set; }
    }
}
