using ContactBook_Domain.Models;
using ContactBook_Services;
using ContactBook_Services.DTOs.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly ContactService _contactService;
        private readonly MailService _mailService;

        public ContactsController(
            ContactService contactService,
            MailService mailService)
        {
            _contactService = contactService;
            _mailService = mailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetContacts()
        {
            var contacts = await _contactService.GetAllAsync();

            return Ok(contacts);
        }

        [HttpGet("page/{pageNumber}")]
        public async Task<ActionResult<List<Contact>>> GetContacts(int pageNumber)
        {
            var (contacts, paginationMetaData) = await _contactService.GetContactsAsyncPagination(pageNumber);

            Response.Headers.TryAdd("X-Pagination", JsonSerializer.Serialize(paginationMetaData));

            return Ok(contacts);
        }

        [HttpGet("{contactId}")]
        public async Task<IActionResult> GetContact(int contactId)
        {
            var contact = await _contactService.GetByIdAsync(contactId);

            return Ok(contact);
        }

        [HttpPost]
        public async Task<IActionResult> AddContact([FromForm]AddContatctDTO addContatctDTO)
        {
            var (state,message) = await _contactService.AddAsync(addContatctDTO);
            
            if (!state)
                return BadRequest(message);


            return Ok(message);

        }

        [HttpPut("{contactId}")]
        public async Task<IActionResult> UpdateContact(int contactId, [FromForm]EditeContactDTO editeContactDTO)
        {
            var (state, message) = await _contactService.UpdateAsync(contactId, editeContactDTO);

            if (!state)
                return BadRequest(message);

            return Ok(message);

        }

        [HttpDelete("{contactId}")]
        public async Task<IActionResult> DeleteContact(int contactId)
        {
            if (!await _contactService.DeleteAsync(contactId))
                return BadRequest("Failed to delete contact");

            return Ok(new JsonResult(new
            {
                title = "Contact deleted",
                message = "Contact has been deleted successfully"
            }));

        }

        [HttpDelete("soft-delete/{contactId}")]
        public async Task<IActionResult> SoftDeleteUser(int contactId)
        {
            if (int.IsNegative(contactId))
                return BadRequest("Invalid contact Id");

            if (!await _contactService.SoftDeleteAsync(contactId))
                return BadRequest("Failed to soft delete contact");

            return Ok(new JsonResult(new
            {
                title = "Contact soft deleted",
                message = "Contact has been deleted successfully"
            }));
        }

        [HttpPost("sendEmail")]
        public async Task<IActionResult> SendEmail(SendEmailDTO sendEmailDTO)
        {
            if (!await _contactService.SendEmail(sendEmailDTO))
                return BadRequest("Email sending failed");

            return Ok(new JsonResult(new
            {
                title = "Email sending success",
                message = "Your email has been send successfully"
            }));

        }

    }
}
