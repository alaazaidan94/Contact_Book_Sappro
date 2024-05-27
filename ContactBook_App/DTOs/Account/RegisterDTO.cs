using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_App.DTOs.Account
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "First name must be at least {2}, and maximum {1} characters")]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Last name must be at least {2}, and maximum {1} characters")]
        public required string LastName { get; set; }

        [Required]
        [DefaultValue("user@sappro.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "The new password must be at least 6 characters long")]
        public required string Password { get; set; }

        [Required]
        [MaxLength(150)]
        public required string CompanyName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string VatNumber { get; set; }

        [Required]
        public required string StreetOne { get; set; }
        public string? StreetTwo { get; set; }

        [Required]
        public required string Country { get; set; }

        [Required]
        public required string City { get; set; }

        [Required]
        public required string State { get; set; }

        [Required]
        [MaxLength(10)]
        public required string Zip { get; set; }

    }

}
