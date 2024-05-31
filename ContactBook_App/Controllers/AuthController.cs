using ContactBook_Services;
using ContactBook_Services.DTOs.Account;
using Microsoft.AspNetCore.Mvc;

namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(
            AuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return BadRequest("Invalid login data provided.");

            var (result, message) = await _authService.CheckAccount(loginDTO);

            if (!result)
                return Unauthorized(message);

            if (!await _authService.ChechConfirmEmail(loginDTO.Email))
                return Unauthorized("Please confrim your email address");

            return await _authService.CreateUserDTO(loginDTO.Email);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (await _authService.CheckEmailExistsAsync(registerDTO.Email))
                return BadRequest($"An existing account is using {registerDTO.Email}, email addres. Please try with another email address");

            if (!await _authService.CreateUserAndCompany(registerDTO))
                return BadRequest("Failed to create user account.");

            try
            {
                if (await _authService.SendConfirmEMailAsync(registerDTO.Email))
                {
                    return Ok(new JsonResult(new { 
                        title = "Account Created", 
                        message = "Your account has been created, please confrim your email address" }));
                }
                return BadRequest("Failed to send email. Please contact admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send email. Please contact admin");
            }

        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (!await _authService.CheckEmailExistsAsync(email))
                return Unauthorized("This email address has not been registered yet");

            if (await _authService.ChechConfirmEmail(email))
                return BadRequest("Your email was confirmed before. Please login to your account");

            if (!await _authService.ConfirmEmail(email, token))
                return BadRequest("Invalid token. Please try again");

            return Ok(new JsonResult(new { 
                title = "Email confirmed", 
                message = "Your email address is confirmed. You can login now" }));

        }

        [HttpPost("resend-confirm-email-link")]
        public async Task<IActionResult> ResendEmailConfirmationLink(EmailDTO emailDTO)
        {
            if (string.IsNullOrEmpty(emailDTO.Email))
                return BadRequest("Invalid email");

            if (!await _authService.CheckEmailExistsAsync(emailDTO.Email))
                return Unauthorized("This email address has not been registered yet");

            if (await _authService.ChechConfirmEmail(emailDTO.Email))
                return BadRequest("Your email was confirmed before. Please login to your account");

            try
            {
                if (await _authService.SendConfirmEMailAsync(emailDTO.Email))
                {
                    return Ok(new JsonResult(new { 
                        title = "Confirmation link sent", 
                        message = "Please confrim your email address" }));

                }
                return BadRequest("Failed to send email. Please contact admin");
            }
            catch (Exception)
            {
                return BadRequest("Failed to send email. Please contact admin");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(EmailDTO emailDTO)
        {
            if (string.IsNullOrEmpty(emailDTO.Email))
                return BadRequest("Invalid email");

            if (!await _authService.CheckEmailExistsAsync(emailDTO.Email))
                return Unauthorized("This email address has not been registered yet");

            if (!await _authService.ChechConfirmEmail(emailDTO.Email))
                return BadRequest("Please confirm your email address first.");

            try
            {
                if (await _authService.SendForgotPasswordEmail(emailDTO.Email))
                {
                    return Ok(new JsonResult(new { 
                        title = "Forgot password email sent", 
                        message = "Please check your email" }));
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
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid email");

            if (!await _authService.CheckEmailExistsAsync(email))
                return Unauthorized("This email address has not been registered yet");

            if (!await _authService.ChechConfirmEmail(email))
                return BadRequest("Please confirm your email address first.");

            try
            {
                if (await _authService.ResetPassword(email, token, setPassword.NewPassword))
                {
                    return Ok(new JsonResult(new { 
                        title = "Password reset success", 
                        message = "Your password has been reset" }));
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
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid email");

            if (!await _authService.CheckEmailExistsAsync(email))
                return Unauthorized("This email address has not been registered yet");

            if (!await _authService.ChechConfirmEmail(email))
                return BadRequest("Please confirm your email address first.");

            try
            {
                if (await _authService.ResetPassword(email, token, setPassword.NewPassword))
                {
                    return Ok(new JsonResult(new { 
                        title = "SetPassword  success", 
                        message = "The password has been set successfully" }));
                }

                return BadRequest("Invalid token. Please try again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid token. Please try again");
            }
        }
    }

}
