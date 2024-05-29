using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Logs;
using Microsoft.EntityFrameworkCore;

namespace ContactBook_Services
{
    public class LogService
    {
        private readonly ContactBookContext _context;
        private readonly IMapper _mapper;

        public LogService(
            ContactBookContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<bool> AddLog(LogModel logModel)
        {
            if (logModel == null)
                return false;
        
            var log = _mapper.Map<LogModel,Log>(logModel);

            var state = await _context.Logs.AddAsync(log);

            if (state.State is EntityState.Added)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Log>> GetAllAsync()
        {
            return await _context.Logs.ToListAsync();
        }
    }
}
