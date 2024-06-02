using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_Services.DTOs.Account
{
    public class RegisterDTO
    {
        [Required]
        [DefaultValue("test")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "First name must be at least {2}, and maximum {1} characters")]
        public required string FirstName { get; set; }

        [Required]
        [DefaultValue("test")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Last name must be at least {2}, and maximum {1} characters")]
        public required string LastName { get; set; }

        [Required]
        [DefaultValue("user@sappro.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }

        [Required]
        [DefaultValue("test@test")]
        [MinLength(6, ErrorMessage = "The new password must be at least 6 characters long")]
        public required string Password { get; set; }

        [Required]
        [DefaultValue("CompanyTest")]
        [MaxLength(150)]
        public required string CompanyName { get; set; }

        [Required]
        [DefaultValue("1155")]
        [MaxLength(100)]
        public required string VatNumber { get; set; }

        [Required]
        [DefaultValue("street1")]
        public required string StreetOne { get; set; }

        [DefaultValue("street2")]
        public string? StreetTwo { get; set; }

        [Required]
        [DefaultValue("Syria")]
        public required string Country { get; set; }

        [Required]
        [DefaultValue("Azaz")]
        public required string City { get; set; }

        [Required]
        [DefaultValue("test")]
        public required string State { get; set; }

        [Required]
        [MaxLength(10)]
        [DefaultValue("12345")]
        public required string Zip { get; set; }

    }

}
