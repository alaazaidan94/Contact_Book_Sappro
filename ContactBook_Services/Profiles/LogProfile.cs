using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Services.DTOs.Logs;

namespace ContactBook_Services.Profiles
{
    public class LogProfile : Profile
    {
        public LogProfile()
        {
            CreateMap<LogModel,Log>();
        }
    }
}
