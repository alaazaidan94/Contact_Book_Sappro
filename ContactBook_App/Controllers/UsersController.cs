using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Services;
using ContactBook_Services.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly UserService _userService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly MailService _mailService;
        private readonly AuthService _authService;

        public UsersController(
            UserManager<User> userManager,
            UserService userService,
            IMapper mapper,
            IConfiguration configuration,
            MailService mailService,
            AuthService authService)
        {
            _userManager = userManager;
            _userService = userService;
            _mapper = mapper;
            _configuration = configuration;
            _mailService = mailService;
            _authService = authService;
        }


        [HttpGet]
        public async Task<ActionResult<List<ViewUserDTO>>> GetUsers()
        {
            var viewUsersDTO = await _userService.GetAllAsync();

            return Ok(viewUsersDTO);
        }

        [HttpGet("page/{pageNumber}")]
        public async Task<ActionResult<List<ViewUserDTO>>> GetUsers(int pageNumber)
        {
            var (viewUsersDTO, paginationMetaData) = await _userService.GetUsersAsyncPagination(pageNumber);

            Response.Headers.TryAdd("X-Pagination", JsonSerializer.Serialize(paginationMetaData));    

            return Ok(viewUsersDTO);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ViewUserDTO>> GetUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User Not Found");

            var viewUserDTO = await _userService.GetByIdAsync(userId);

            return Ok(viewUserDTO);
        }

        [HttpPost]
        public async Task<ActionResult<ViewUserDTO>> InviteUser(InviteUserDTO inviteUserDTO)
        {
            if (inviteUserDTO == null)
                return BadRequest("Invaild model");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null)
                return BadRequest("Faild to get current User");

            if (!await _userService.InviteUser(currentUserId, inviteUserDTO))
                return BadRequest("Invalid Invite User");

            try
            {
                if (await _authService.SendConfirmAndSetPasswordAsync(inviteUserDTO.Email))
                {
                    return Ok(new JsonResult(new { 
                        title = "Invite Success", 
                        message = "The invitation has been sent successfully, please confrim your email address and set password" }));
                }

                return BadRequest("Failed to send email. Please contact admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send email. Please contact admin");
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> EditUser(string userId, EditUserDTO editUserDTO)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Invalid UserId");

            if (editUserDTO == null)
                return BadRequest("Invalid model");

            if (!await _userService.UpdateAsync(userId, editUserDTO))
                return BadRequest("Update failed");

            return Ok(new JsonResult(new
            {
                title = "User modified",
                message = "User has been modified successfully"
            }));
        }

        [HttpDelete("soft-delete/{userId}")]
        public async Task<IActionResult> SoftDeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Invalid userId");

            if (! await _userService.SoftDeleteAsync(userId))
                return BadRequest("Invalid Deleted");

            return Ok(new JsonResult(new
            {
                title = "User deleted",
                message = "User has been deleted successfully"
            }));
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Invalid userId");

            if (!await _userService.DeleteAsync(userId))
                return BadRequest("Invalid Deleted");

            return Ok(new JsonResult(new
            {
                title = "User deleted",
                message = "User has been deleted successfully"
            }));
        }

    }
}
