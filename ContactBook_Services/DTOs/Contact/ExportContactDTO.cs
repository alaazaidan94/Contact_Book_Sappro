using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_Services.DTOs.Contact
{
    public class ExportContactDTO
    {
        [Required]
        [DefaultValue("user@sappro.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }

        [Required]
        public required List<int> contactIds { get; set; } = new List<int>();
    }
}
