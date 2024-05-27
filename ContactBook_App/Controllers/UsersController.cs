using AutoMapper;
using ContactBook_App.DTOs;
using ContactBook_App.DTOs.Company;
using ContactBook_App.DTOs.Contact;
using ContactBook_App.DTOs.Users;
using ContactBook_Domain.Models;
using ContactBook_Services.AccountServices;
using ContactBook_Services.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;

namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly AuthService _authService;

        public UsersController(
            UserManager<User> userManager,
            IUserRepository userRepo,
            IMapper mapper,
            IConfiguration configuration,
            IMailService mailService,
            AuthService authService)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _mapper = mapper;
            _configuration = configuration;
            _mailService = mailService;
            _authService = authService;
        }


        [HttpGet]
        public async Task<ActionResult<List<ViewUserDTO>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var viewUserDTO = _mapper.Map<List<ViewUserDTO>>(users);
            return Ok(viewUserDTO);
        }

        [HttpGet("page/{pageNumber}")]
        public async Task<ActionResult<List<ViewUserDTO>>> GetUsers(int pageNumber)
        {
            int pageSize = Convert.ToInt32(_configuration.GetSection("maxPageSize").Value);

            var (users, paginationMetaData) = await _userRepo.GetUsersAsyncPagination(pageNumber, pageSize);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetaData));

            // mapping object user for viewUserDTO
            var viewUserDTO = _mapper.Map<List<ViewUserDTO>>(users);

            return Ok(viewUserDTO);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ViewUserDTO>> GetUser(string userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return NotFound("User Not Found");

            var viewUserDTO = _mapper.Map<ViewUserDTO>(user);

            return Ok(viewUserDTO);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ViewUserDTO>> InviteUser(InviteUserDTO inviteUserDTO)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest("User Not Found");

            var currentUuser = await _userManager.FindByIdAsync(userId);

            if (currentUuser is null)
                return BadRequest("User Not Found");

            var companyId = currentUuser.CompanyId;

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
                return BadRequest(result.Errors);

            try
            {
                if (await _authService.SendConfirmAndSetPasswordAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Invite Success", message = "The invitation has been sent successfully, please confrim your email address and set password" }));
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
                return BadRequest("Invalid User");

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return BadRequest("User Not Found");

            _mapper.Map(editUserDTO, user);

            var result = await _userRepo.UpdateAsync(user);
            if (!result)
                return BadRequest("Invalid Edit User");

            return Ok(user);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _userRepo.SoftDeleteAsync(userId);
            if (!result)
                return BadRequest("Invalid Deleted");

            return NoContent();
        }

        [HttpPost("sendEmail")]
        public async Task<IActionResult> SendEmail(SendEmailDTO sendEmailDTO)
        {

           await _mailService.SendEmail(sendEmailDTO.ToEmail,sendEmailDTO.Subject,sendEmailDTO.Body);

            return Ok();
           
        }
    }
}
