using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Services.DTOs.Contact;

namespace ContactBook_Services.Profiles
{
    public class ContactProfile : Profile
    {
        public ContactProfile()
        {
            CreateMap<AddContatctDTO,Contact>();
        }
    }
}
