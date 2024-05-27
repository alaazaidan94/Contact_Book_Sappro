using AutoMapper;
using ContactBook_App.DTOs.Account;
using ContactBook_App.DTOs.Company;
using ContactBook_Domain.Models;

namespace ContactBook_App.Profiles
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
