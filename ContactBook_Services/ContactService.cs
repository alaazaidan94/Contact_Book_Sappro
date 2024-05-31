using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Contact;
using ContactBook_Services.DTOs.Logs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ContactBook_Services
{
    public class ContactService
    {
        private readonly ContactBookContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly LogService _logService;
        private readonly MailService _mailService;
        private readonly IMapper _mapper;
        private readonly ILogger<Contact> _logger;

        public ContactService(
            ContactBookContext context,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            LogService logService,
            MailService mailService,
            IMapper mapper,
            ILogger<Contact> logger)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logService = logService;
            _mailService = mailService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<Contact>> GetAllAsync()
        {
            return await _context.Contacts.ToListAsync();
        }
        public async Task<(List<Contact>, PaginationMetaData)> GetContactsAsyncPagination(int pageNumber)
        {
            int pageSize = Convert.ToInt32(_configuration.GetSection("maxPageSize").Value);

            var query = _context.Contacts.AsQueryable();

            // Get total count of contacts
            var totalCount = await query.CountAsync();

            // Create Object Pagination
            var paginationMetaDate = new PaginationMetaData(totalCount, pageSize, pageNumber);

            var contacts = await query
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();


            return (contacts, paginationMetaDate);
        }
        public async Task<Contact> GetByIdAsync(int id)
        {
            return await _context.Contacts.FirstOrDefaultAsync(p => p.ContactId == id);
        }
        public async Task<(bool success, string message)> AddAsync(AddContatctDTO addContactDTO)
        {
            // Retrieve the current user
            var user = await GetCurrentUser();
            if (user == null)
            {
                return (false, "User Not Found");
            }

            // Get the company ID of the current user
            var companyId = user.CompanyId;
            string imageUrl = string.Empty;

            if (addContactDTO.UploadImage != null)
            {
                // Upload the image
                (bool result, string message) = await UploadImage(addContactDTO.UploadImage);
                if (!result)
                    return (false, message);
             
                imageUrl = message;
            }

            // Map the AddContactDTO to a Contact entity using AutoMapper
            var contact = _mapper.Map<AddContatctDTO, Contact>(addContactDTO, opts =>
            {
                opts.AfterMap((src, dest) =>
                {
                    dest.ImageUrl = imageUrl;
                    dest.CompanyId = companyId;
                });
            });

            await _context.Contacts.AddAsync(contact);
            try
            {
                await _context.SaveChangesAsync();

                // Create a new log entry for adding the contact
                var log = new LogModel
                {
                    ContactName = $"{contact.FirstName} {contact.LastName}",
                    Action = LogAction.Add,
                    ActionBy = $"{user.FirstName} {user.LastName}",
                };
                await _logService.AddLog(log);

                return (true, "Create contact success");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while create contact operation");

                return (false, "Failed to create contact");
            }
        }
        public async Task<(bool success, string message)> UpdateAsync(int contactId, EditeContactDTO editContactDTO)
        {
            // Retrieve the current user
            var user = await GetCurrentUser();

            if (user == null)
                return (false, "User Not Found");

            // Find the contact by its ID
            var contact = await _context.Contacts.FindAsync(contactId);

            if (contact == null)
                return (false, "Contact Not Found");

            var imageUrl = contact.ImageUrl;

            if (editContactDTO.UploadImage != null)
            {
                // Upload the new image
                (bool result, string message) = await UploadImage(editContactDTO.UploadImage, imageUrl!);
              
                if (!result)
                    return (false, message);

                imageUrl = message;
            }

            // Map the EditContactDTO to the existing Contact entity using AutoMapper
            _mapper.Map(editContactDTO, contact, opts =>
            {
                opts.AfterMap((src, dest) =>
                {
                    dest.ImageUrl = imageUrl;
                });
            });

            _context.Contacts.Update(contact);

            try
            {
                await _context.SaveChangesAsync();

                // Create a new log entry for updating the contact
                var log = new LogModel
                {
                    ContactName = $"{contact.FirstName} {contact.LastName}",
                    Action = LogAction.Update,
                    ActionBy = $"{user.FirstName} {user.LastName}",
                };
                await _logService.AddLog(log);

                return (true, "Update contact success");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while update contact operation");

                return (false, "Failed to update contact");
            }
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetCurrentUser();
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
                return false;

            _context.Contacts.Remove(contact);

            try
            {
                _context.SaveChanges();

                var log = new LogModel
                {
                    ContactName = $"{contact.FirstName} {contact.LastName}",
                    Action = LogAction.Delete,
                    ActionBy = $"{user.FirstName} {user.LastName}"
                };

                await _logService.AddLog(log);

                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while delete operation for contact with ID {ContactId}.", id);
                return false;
            }
        }
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var user = await GetCurrentUser();

            var contact = await _context.Contacts.SingleOrDefaultAsync(c => c.ContactId == id);

            if (contact == null)
                return false;

            contact.isDeleted = true;
            _context.Update(contact);

            try
            {
               await _context.SaveChangesAsync();

                var log = new LogModel
                {
                    ContactName = $"{contact.FirstName} {contact.LastName}",
                    Action = LogAction.SoftDelete,
                    ActionBy = $"{user.FirstName} {user.LastName}"
                };

                await _logService.AddLog(log);

                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while soft delete operation for contact with ID {ContactId}.", id);
                return false;
            }

        }
        public async Task<bool> SendEmail(SendEmailDTO sendEmailDTO)
        {
            var user = await GetCurrentUser();
            var contact = await GetContactByEmail(sendEmailDTO.To);

            if (contact == null)
                return false;

            if (!await _mailService.SendContactEmail(sendEmailDTO))
                return false;

            var log = new LogModel()
            {
                ContactName = contact.FirstName + " " + contact.LastName,
                Action = LogAction.EmailSent,
                ActionBy = user.FirstName + " " + user.LastName,
            };

            await _logService.AddLog(log);

            return true;

        }
        public async Task<(bool success, string message)> UploadImage(IFormFile image, string imageUrl = "")
        {
            // Delete the old image if the current image URL is provided
            if (!string.IsNullOrEmpty(imageUrl))
            {
                string filePath = Path.GetFileName(imageUrl);

                // Check if the file exists and delete it if it does
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Validate the extension
            List<string> validExtensions = new List<string> { ".jpg", ".png" };

            string extension = Path.GetExtension(image.FileName).ToLower();

            if (!validExtensions.Contains(extension))
                return (false, $"Extension is not valid ({string.Join(", ", validExtensions)})");

            // Check if the file size is less than 5MB
            long size = image.Length;

            if (size > (5 * 1024 * 1024)) // 5MB
                return (false, "Maximum size can be 5MB");


            // Create a unique name for the new file using a GUID
            string imageName = Guid.NewGuid().ToString() + extension;

            string path = GetImagesPath();

            // Ensure the directory exists and create it if it doesn't
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string imagePath = Path.Combine(path, imageName);

            // Save the image to the specified path using FileStream
            using (FileStream stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return (true, imagePath);
        }


        private string GetImagesPath()
        {
            return _environment.WebRootPath + "\\Uplodes\\Contact-Image";
        }
        private async Task<User> GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

            return user;
        }
        private async Task<Contact> GetContactByEmail(string email)
        {
            return await _context.Contacts.SingleOrDefaultAsync(c => c.EmailOne == email || c.EmailTwo == email);
        }
    }
}
