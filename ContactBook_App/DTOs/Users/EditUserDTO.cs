using ContactBook_Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_App.DTOs.Users
{
    public class EditUserDTO
    {
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [MaxLength(100)]
        public required string LastName { get; set; }

        [MaxLength(100)]
        public required string VatNumber { get; set; }

        public UserStatus Status { get; set; }
        public Roles Role { get; set; }
    }
}
