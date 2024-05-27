using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_App.DTOs.Account
{
    public class EmailDTO
    {
        [Required]
        [DefaultValue("user@sappro.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }
    }
}
