using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContactBook_Services.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ContactBookContext _context;
        private readonly UserManager<User> _userManager;

        public UserRepository(
            ContactBookContext context,
            UserManager<User> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<List<User>> GetAllAsync()
        {

            return await _context.Users.ToListAsync();
        }

        public async Task<(List<User>, PaginationMetaData)> GetUsersAsyncPagination(int pageNumber, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            // Get total count of users
            var totalCount = await query.CountAsync();

            // Create Object Pagination
            var paginationMetaDate = new PaginationMetaData(totalCount, pageSize, pageNumber);

            var users = await query
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (users, paginationMetaDate);
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> AddAsync(User entity)
        {
            var state = await _context.Users.AddAsync(entity);

            if (state.State is EntityState.Added)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            var state = _context.Users.Update(entity);

            if (state.State is EntityState.Modified)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(p => p.Id == id);

            if (user == null)
                return false;

            user.isDeleted = true;

            var state = _context.Update(user);

            if (state.State is EntityState.Modified)
            {
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }
        public Task SaveChanges()
        {
            throw new NotImplementedException();
        }
    }
}
