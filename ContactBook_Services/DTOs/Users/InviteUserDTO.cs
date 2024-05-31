using ContactBook_Domain.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ContactBook_Services.DTOs.Users
{
    public class InviteUserDTO
    {
        [Required]
        [StringLength(100,MinimumLength = 1, ErrorMessage = "First name must be at least {1}")]
        [DefaultValue("John")]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be at least {1}")]
        [DefaultValue("Doe")]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [DefaultValue("user@example.com")]
        public required string Email { get; set; }

        [Required]
        [DefaultValue("+1234567890")]
        public required string PhoneNumber { get; set; }

        [Required]
        [DefaultValue(Roles.User)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required Roles Role { get; set; } = Roles.User;
    }
}
