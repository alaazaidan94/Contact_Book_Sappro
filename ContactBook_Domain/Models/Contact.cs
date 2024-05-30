using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ContactBook_Domain.Models
{
    public class Contact
    {
        public int ContactId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string EmailOne { get; set; }

        [EmailAddress]
        public string? EmailTwo { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Mobile { get; set; }

        [Required]
        public required string AddressOne { get; set; }
        public string? AddressTwo { get; set; }

        public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContactState ContactState { get; set; } = ContactState.Active;
        public bool isFavorite { get; set; } = false;
        public bool isDeleted { get; set; } = false;
        public int CompanyId { get; set; }
        public Company Company { get; set; }

    }

    public enum ContactState
    {
        Active = 1,
        Inactive = 2
    }
}
