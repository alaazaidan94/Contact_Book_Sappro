using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Company;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace ContactBook_Services
{
    public class CompanyService
    {
        private readonly ContactBookContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public CompanyService(
            ContactBookContext context,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies.ToListAsync();
        }
        public async Task<Company> GetByIdAsync(int id)
        {
            return await _context.Companies.FirstOrDefaultAsync(p => p.CompanyId == id);

        }
        public async Task<bool> AddAsync(Company entity)
        {
            var state = await _context.Companies.AddAsync(entity);

            if (state.State is EntityState.Added)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> UpdateAsync(string currentUserId, EditCompanyDTO editCompanyDTO)
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser == null)
                return false;

            var company = await GetByIdAsync(currentUser.CompanyId);

            if (company == null)
                return false;

            _mapper.Map(editCompanyDTO, company);

            var state = _context.Companies.Update(company);

            if (state.State is EntityState.Modified)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == id);
            if (company == null)
                return false;

            var state = _context.Remove(company);

            if (state.State is EntityState.Deleted)
            {
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }


    }
}
