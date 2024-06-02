using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_Services.DTOs.Company
{
    public class EditCompanyDTO
    {
        [Required]
        [DefaultValue("CompanyTest")]
        [MaxLength(150)]
        public required string CompanyName { get; set; }

        [Required]
        [DefaultValue("1155")]
        [MaxLength(20)]
        public required string VatNumber { get; set; }

        [Required]
        [DefaultValue("street1")]
        public required string StreetOne { get; set; }

        [DefaultValue("street2")]
        public string? StreetTwo { get; set; }

        [Required]
        [DefaultValue("Syria")]
        public required string Country { get; set; }

        [Required]
        [DefaultValue("Azaz")]
        public required string City { get; set; }

        [Required]
        [DefaultValue("test")]
        public required string State { get; set; }

        [Required]
        [MaxLength(10)]
        [DefaultValue("12345")]
        public required string Zip { get; set; }
    }
}
