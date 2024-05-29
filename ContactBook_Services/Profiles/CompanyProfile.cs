using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Services.DTOs.Account;
using ContactBook_Services.DTOs.Company;

namespace ContactBook_Services.Profiles
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile()
        {
            CreateMap<RegisterDTO, Company>();
            CreateMap<EditCompanyDTO, Company>();
        }
    }
}
