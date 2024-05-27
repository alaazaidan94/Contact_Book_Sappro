using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace ContactBook_Services.Repository
{
    public class CompanyRepository : IRepository<Company,int>
    {
        private readonly ContactBookContext _context;

        public CompanyRepository(ContactBookContext context)
        {
            this._context = context;
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

        public async Task<bool> UpdateAsync(Company entity)
        {
            var state = _context.Companies.Update(entity);

            if (state.State is EntityState.Modified)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(p => p.CompanyId == id);

            if (company == null)
                return false;

            company.isDeleted = true;

            var state = _context.Update(company);

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
        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
