using AutoMapper;
using ContactBook_App.DTOs.Account;
using ContactBook_App.DTOs.Users;
using ContactBook_Domain.Models;

namespace ContactBook_App.Profiles
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
