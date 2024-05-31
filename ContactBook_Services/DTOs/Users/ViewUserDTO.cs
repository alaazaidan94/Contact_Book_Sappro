using ContactBook_Domain.Models;
using System.Text.Json.Serialization;

namespace ContactBook_Services.DTOs.Users
{
    public class ViewUserDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Roles Role { get; set; }

        public bool isDeleted { get; set; }
    }
}
