using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContactBook_Services
{
    public class UserService
    {
        private readonly ContactBookContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserService(
            ContactBookContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            IMapper mapper
            )
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
        }


        public async Task<List<ViewUserDTO>> GetAllAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            var viewUserDTO = _mapper.Map<List<ViewUserDTO>>(users);

            return viewUserDTO;
        }
        public async Task<(List<ViewUserDTO>, PaginationMetaData)> GetUsersAsyncPagination(int pageNumber)
        {
            int pageSize = Convert.ToInt32(_configuration.GetSection("maxPageSize").Value);

            var query = _context.Users.AsQueryable();

            // Get total count of users
            var totalCount = await query.CountAsync();

            // Create Object Pagination
            var paginationMetaDate = new PaginationMetaData(totalCount, pageSize, pageNumber);

            var users = await query
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            var viewUsersDTO = _mapper.Map<List<ViewUserDTO>>(users);

            return (viewUsersDTO, paginationMetaDate);
        }
        public async Task<ViewUserDTO> GetByIdAsync(string id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

            var viewUserDTO = _mapper.Map<ViewUserDTO>(user);

            return viewUserDTO;
        }
        public async Task<bool> InviteUser(string currentUserId, InviteUserDTO inviteUserDTO)
        {
            var currentUser = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == currentUserId);
            
            if (currentUser == null)
                return false;

            var companyId = currentUser.CompanyId;

            var user = _mapper.Map<InviteUserDTO, User>(inviteUserDTO, opts =>
            {
                opts.AfterMap((src, dest) =>
                {
                    dest.UserName = inviteUserDTO.Email;
                    dest.CompanyId = companyId;
                });
            });

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
                return false;

            return true;
        }
        public async Task<bool> UpdateAsync(string userId, EditUserDTO editUserDTO)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            _mapper.Map(editUserDTO, user);

            var state = await _userManager.UpdateAsync(user);

            if (!state.Succeeded)
                return false;

            return true;
        }
        public async Task<bool> SoftDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return false;

            user.isDeleted = true;

            var state = await _userManager.UpdateAsync(user);

            if (!state.Succeeded)
                return false;

            return true;
        }
        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return false;

            var state = await _userManager.DeleteAsync(user);

            if (!state.Succeeded)
                return false;

            return true;
        }
    }
}
