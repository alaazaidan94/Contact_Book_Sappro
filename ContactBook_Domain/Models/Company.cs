using System.ComponentModel.DataAnnotations;

namespace ContactBook_Domain.Models
{
    public class Company
    {
        public required int CompanyId { get; set; }

        [MaxLength(150)]
        public required string CompanyName { get; set; }
        [MaxLength(20)]
        public required string VatNumber { get; set;}
        public required string StreetOne { get; set;}
        public string? StreetTwo { get; set;}
        public required string Country { get; set;}
        public required string City { get; set;}
        public required string State { get; set;}

        [MaxLength(10)]
        public required string Zip { get; set;}

        public bool isDeleted { get; set; } = false;
    }
}
