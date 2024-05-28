using AutoMapper;
using ContactBook_App.DTOs.Account;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.AccountServices;
using ContactBook_Services.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly IRepository<Company, int> _companyRepo;
        private readonly ContactBookContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly AuthService _authService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IMapper mapper,
            IRepository<Company, int> companyRepo,
            ContactBookContext context,
            IConfiguration configuration,
            IMailService mailService,
            AuthService authService
            )
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._mapper = mapper;
            this._companyRepo = companyRepo;
            this._context = context;
            _configuration = configuration;
            _mailService = mailService;
            _authService = authService;
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null)
                return Unauthorized("Invalid username or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid username or password");


            if (user.Status == UserStatus.Pending || user.EmailConfirmed == false)
                return Unauthorized("Please confrim your email address");

            return await CreateUserDTO(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (await _authService.CheckEmailExistsAsync(registerDTO.Email))
                return BadRequest($"An existing account is using {registerDTO.Email}, email addres. Please try with another email address");

            Company company = _mapper.Map<Company>(registerDTO);

            if (!await _companyRepo.AddAsync(company))
                return BadRequest("Register Faild");


            User user = _mapper.Map<RegisterDTO, User>(registerDTO, opts =>
            {
                opts.AfterMap((src, dest) =>
                {
                    dest.CompanyId = company.CompanyId;
                    dest.UserName = registerDTO.Email;
                });
            });

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                await _companyRepo.DeleteAsync(company.CompanyId);
                return BadRequest(result.Errors);
            }


            try
            {
                if (await _authService.SendConfirmEMailAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Account Created", message = "Your account has been created, please confrim your email address" }));
                }

                return BadRequest("Failed to send email. Please contact admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send email. Please contact admin");
            }


        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Unauthorized("This email address has not been registered yet");

            if (user.EmailConfirmed == true)
                return BadRequest("Your email was confirmed before. Please login to your account");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (result.Succeeded)
                {
                    user.Status = UserStatus.Active;
                    await _userManager.UpdateAsync(user);
                    return Ok(new JsonResult(new { title = "Email confirmed", message = "Your email address is confirmed. You can login now" }));
                }

                return BadRequest("Invalid token. Please try again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid token. Please try again");
            }

        }

        [HttpPost("resend-confirm-email-link")]
        public async Task<IActionResult> ResendEmailConfirmationLink(EmailDTO emailDTO)
        {
            if (string.IsNullOrEmpty(emailDTO.Email))
                return BadRequest("Invalid email");

            var user = await _userManager.FindByEmailAsync(emailDTO.Email);

            if (user == null)
                return Unauthorized("This email address has not been registerd yet");

            if (user.EmailConfirmed == true)
                return BadRequest("Your email address was confirmed before. Please login to your account");

            try
            {
                if (await _authService.SendConfirmEMailAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Confirmation link sent", message = "Please confrim your email address" }));
                }

                return BadRequest("Failed to send email. PLease contact admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send email. PLease contact admin");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(EmailDTO emailDTO)
        {
            if (string.IsNullOrEmpty(emailDTO.Email))
                return BadRequest("Invalid email");

            var user = await _userManager.FindByEmailAsync(emailDTO.Email);

            if (user == null)
                return Unauthorized("This email address has not been registerd yet");

            if (user.EmailConfirmed == false)
                return BadRequest("Please confirm your email address first.");

            try
            {
                if (await _authService.SendForgotPasswordEmail(user))
                {
                    return Ok(new JsonResult(new { title = "Forgot password email sent", message = "Please check your email" }));
                }

                return BadRequest("Failed to send email. Please contact admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send email. Please contact admin");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string email, string token, SetPasswordDTO setPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return Unauthorized("This email address has not been registerd yet");

            if (user.EmailConfirmed == false)
                return BadRequest("PLease confirm your email address first");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, setPassword.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Password reset success", message = "Your password has been reset" }));
                }

                return BadRequest("Invalid token. Please try again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid token. Please try again");
            }
        }

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword(string email, string token, SetPasswordDTO setPassword)
        {
            var user = await _userManager.FindByIdAsync(email);

            if (user == null)
                return Unauthorized("This email address has not been registerd yet");

            if (user.EmailConfirmed == false)
                return BadRequest("PLease confirm your email address first");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, setPassword.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "SetPassword  success", message = "The password has been set successfully" }));
                }

                return BadRequest("Invalid token. Please try again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid token. Please try again");
            }
        }


        private Task<UserDTO> CreateUserDTO(User user)
        {
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
            var days = int.Parse(_configuration["JWT:AddDays"]!);

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(days),
                SigningCredentials = credentials,
                Issuer = _configuration["JWT:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            var userDTO = new UserDTO
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role,
                Exp = jwt.ValidTo,
                Token = tokenHandler.WriteToken(jwt)
            };

            return Task.FromResult(userDTO);
        }


    }

}
