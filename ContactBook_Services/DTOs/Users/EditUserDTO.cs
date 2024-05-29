using ContactBook_Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_Services.DTOs.Users
{
    public class EditUserDTO
    {
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [MaxLength(100)]
        public required string LastName { get; set; }

        [MaxLength(100)]
        public required string PhoneNumber { get; set; }

        public UserStatus Status { get; set; }
        public Roles Role { get; set; }
    }
}
