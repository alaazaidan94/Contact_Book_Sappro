using System.ComponentModel.DataAnnotations;

namespace ContactBook_Domain.Models
{
    public class Contact
    {

        public int ContactId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailOne { get; set; }

        [EmailAddress]
        public string EmailTwo { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        public string Mobile { get; set; }
        public string AddressOne { get; set; }
        public string AddressTwo { get; set; }

        public int CompanyId { get; set; }
    }
}
