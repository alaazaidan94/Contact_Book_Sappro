using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Services.DTOs.Account;
using ContactBook_Services.DTOs.Users;

namespace ContactBook_Services.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterDTO, User>();
            CreateMap<User, ViewUserDTO>();
            CreateMap<InviteUserDTO, User>();
            CreateMap<EditUserDTO, User>();
        }
    }
}
