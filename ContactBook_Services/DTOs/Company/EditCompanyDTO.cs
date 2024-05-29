using System.ComponentModel.DataAnnotations;

namespace ContactBook_Services.DTOs.Company
{
    public class EditCompanyDTO
    {
        [Required]
        [MaxLength(150)]
        public required string CompanyName { get; set; }

        [Required]
        [MaxLength(20)]
        public required string VatNumber { get; set; }

        [Required]
        public required string StreetOne { get; set; }

        public string? StreetTwo { get; set; }

        [Required]
        public required string Country { get; set; }

        [Required]
        public required string City { get; set; }

        [Required]
        public required string State { get; set; }

        [Required]
        [MaxLength(10)]
        public required string Zip { get; set; }
    }
}
