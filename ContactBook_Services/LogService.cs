using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Logs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactBook_Services
{
    public class LogService
    {
        private readonly ContactBookContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<Log> _logger;

        public LogService(
            ContactBookContext context,
            IMapper mapper,
            ILogger<Log> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<bool> AddLog(LogModel logModel)
        {
            if (logModel == null)
                return false;

            var log = _mapper.Map<LogModel, Log>(logModel);

            try
            {
                await _context.Logs.AddAsync(log);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while adding a Log to the database.");

                return false;
            }
        }

        public async Task<List<Log>> GetAllAsync()
        {
            return await _context.Logs.ToListAsync();
        }
    }
}
