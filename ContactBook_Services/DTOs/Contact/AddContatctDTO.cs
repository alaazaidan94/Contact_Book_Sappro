
using ContactBook_Domain.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_Services.DTOs.Contact
{
    public class AddContatctDTO
    {
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string EmailOne { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string? EmailTwo { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Mobile { get; set; }

        [Required]
        public required string AddressOne { get; set; }
        public string? AddressTwo { get; set; }
        public IFormFile? UploadImage { get; set; }
    }
}
