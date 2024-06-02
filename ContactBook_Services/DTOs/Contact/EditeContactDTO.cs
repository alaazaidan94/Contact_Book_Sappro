using ContactBook_Domain.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ContactBook_Services.DTOs.Contact
{
    public class EditeContactDTO
    {
        [Required]
        [DefaultValue("test")]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [DefaultValue("test")]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [Required]
        [DefaultValue("user@sappro.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string EmailOne { get; set; }

        [DefaultValue("user@sappro.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string? EmailTwo { get; set; }

        [DefaultValue("001155")]
        public string? PhoneNumber { get; set; }

        [DefaultValue("00963xxxxxxx")]
        public string? Mobile { get; set; }

        [Required]
        [DefaultValue("test")]
        public required string AddressOne { get; set; }

        [DefaultValue("test")]
        public string? AddressTwo { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContactState ContactState { get; set; } = ContactState.Active;
        public bool isFavorite { get; set; } = false;

        public IFormFile? UploadImage { get; set; }
    }
}
