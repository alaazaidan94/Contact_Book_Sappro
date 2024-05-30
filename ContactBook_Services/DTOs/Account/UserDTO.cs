using ContactBook_Domain.Models;
using System.Text.Json.Serialization;

namespace ContactBook_Services.DTOs.Account
{
    public class UserDTO
    {
        public string UserId { get; set; }
        public string FullName { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Roles Role { get; set; }
        public DateTime Exp { get; set; }
        public string Token { get; set; }
    }
}
