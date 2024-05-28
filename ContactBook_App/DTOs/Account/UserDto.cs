using ContactBook_Domain.Models;

namespace ContactBook_App.DTOs.Account
{
    public class UserDTO
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public Roles Role { get; set; }
        public DateTime Exp { get; set; }
        public string Token { get; set; }
    }
}
