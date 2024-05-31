using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Company;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;


namespace ContactBook_Services
{
    public class CompanyService
    {
        private readonly ContactBookContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<Company> _logger;

        public CompanyService(
            ContactBookContext context,
            UserManager<User> userManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<Company> logger)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies.ToListAsync();
        }
        public async Task<Company> GetByIdAsync(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(p => p.CompanyId == id);
            return company!;

        }
        public async Task<bool> AddAsync(Company entity)
        {
            try
            {
                await _context.Companies.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while adding a company to the database.");
                return false;
            }
        }
        public async Task<bool> UpdateAsync(EditCompanyDTO editCompanyDTO)
        {
            var currentUser = await GetCurrentUser();

            if (currentUser == null)
                return false;

            var company = await GetByIdAsync(currentUser.CompanyId);

            if (company == null)
                return false;

            _mapper.Map(editCompanyDTO, company);

            try
            {
                _context.Companies.Update(company);
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while updating a company to the database.");

                return false;
            }
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == id);
            if (company == null)
                return false;

            try
            {
                _context.Companies.Remove(company);
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while Deleting a company from the database.");

                return false;
            }
        }

        private async Task<User> GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

            return user!;
        }
    }
}
