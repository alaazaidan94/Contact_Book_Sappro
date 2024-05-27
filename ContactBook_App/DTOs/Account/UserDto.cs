using ContactBook_Domain.Models;

namespace ContactBook_App.DTOs.Account
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public Roles Role { get; set; }
        public string Token { get; set; }
    }
}
