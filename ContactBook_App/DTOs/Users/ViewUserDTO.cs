using ContactBook_Domain.Models;

namespace ContactBook_App.DTOs.Users
{
    public class ViewUserDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
    }
}
