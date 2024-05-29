using AutoMapper;
using ContactBook_Services.DTOs.Logs;
using System.Diagnostics;

namespace ContactBook_Services.Profiles
{
    public class LogProfile : Profile
    {
        public LogProfile()
        {
            CreateMap<LogModel,Activity>();
        }
    }
}
