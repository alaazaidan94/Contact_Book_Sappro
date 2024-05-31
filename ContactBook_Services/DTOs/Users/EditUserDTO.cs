using ContactBook_Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ContactBook_Services.DTOs.Users
{
    public class EditUserDTO
    {
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [MaxLength(100)]
        public required string LastName { get; set; }

        [MaxLength(20)]
        public required string PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Roles Role { get; set; }
    }
}
