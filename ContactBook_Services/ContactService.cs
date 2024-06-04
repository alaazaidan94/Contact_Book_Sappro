using AutoMapper;
using Azure;
using ClosedXML.Excel;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Contact;
using ContactBook_Services.DTOs.Logs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
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
            var user = await GetCurrentUser();
            var companyId = user.CompanyId;

            return await _context.Contacts.Where(c => c.CompanyId == companyId).ToListAsync();
        }
        public async Task<(List<Contact>, PaginationMetaData)> GetContactsAsyncPagination(int pageNumber)
        {
            var user = await GetCurrentUser();
            var companyId = user.CompanyId;

            int pageSize = Convert.ToInt32(_configuration.GetSection("maxPageSize").Value);

            var query = _context.Contacts.Where(c => c.CompanyId == companyId).AsQueryable();

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
            var user = await GetCurrentUser();
            var contact = await _context.Contacts.FirstOrDefaultAsync(p => p.ContactId == id);

            var log = new LogModel
            {
                ContactName = $"{contact!.FirstName} {contact.LastName}",
                Action = LogAction.Access,
                ActionBy = $"{user.FirstName} {user.LastName}",
                CompanyId = user.CompanyId
            };

            await _logService.AddLog(log);

            return contact!;
        }
        public async Task<(bool success, string message)> AddAsync(AddContatctDTO addContactDTO)
        {
            // Get current user
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
                    CompanyId = user.CompanyId,
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
                    CompanyId = user.CompanyId
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

        public async Task<(bool success, string message)> EditIsFavorate(int contactId)
        {
            var contact = await _context.Contacts.FindAsync(contactId);
            if (contact == null)
                return (false, "Contact not found.");

            contact.isFavorite = !contact.isFavorite;

            _context.Contacts.Update(contact);

            _context.SaveChanges();

            return (true,"Success");
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetCurrentUser();
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
                return false;

            DeleteImage(contact.ImageUrl!);

            _context.Contacts.Remove(contact);

            try
            {
                _context.SaveChanges();

                var log = new LogModel
                {
                    ContactName = $"{contact.FirstName} {contact.LastName}",
                    Action = LogAction.Delete,
                    ActionBy = $"{user.FirstName} {user.LastName}",
                    CompanyId = user.CompanyId
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
                    ActionBy = $"{user.FirstName} {user.LastName}",
                    CompanyId = user.CompanyId
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
                CompanyId = user.CompanyId
            };

            await _logService.AddLog(log);

            return true;

        }
        public async Task<(bool success, string message)> UploadImage(IFormFile image, string imageUrl = "")
        {
            // Delete the old image if the current image URL is provided
            if (!string.IsNullOrEmpty(imageUrl))
            {
                // Check if the file exists and delete it if it does
                DeleteImage(_environment.WebRootPath + "\\" + imageUrl);
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

            string imagePath = _configuration["ImagePath"]!;

            string rootpath = _environment.WebRootPath;

            string fullPath = rootpath +"\\" + imagePath;

            // Ensure the directory exists and create it if it doesn't
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            string imageFullPath = Path.Combine(fullPath, imageName);

            // Save the image to the specified path using FileStream
            try
            {
                using (FileStream stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return (false, "An error occurred while create image. "+ex.Message);
            }

            return (true, imagePath +"\\"+ imageName);
        }
        public async Task<(bool success, string message)> DownloadImage(int contactId)
        {
            // Fetch the contact from the database by ID
            var contact = await _context.Contacts.FindAsync(contactId);

            if (contact == null)
                return (false, "Contact not found");

            // Get the host URL from the configuration
            var hostUrl = _configuration["appUrl"];

            var fullPath = _environment.WebRootPath +"\\" + contact.ImageUrl;

            // Check if the image file exists locally
            if (!File.Exists(fullPath))
                return (false, "Image not found");

            var imageURL = hostUrl + "/" + contact.ImageUrl!.Replace("\\", "/");

            return (true, imageURL);

        }
        public async Task<(bool success, string message)> ExportContact(ExportContactDTO exportContactDTO)
        {
            var user = await GetCurrentUser();
            if (user == null) 
                return (false, "Current user not found");

            var company = await _context.Companies.FindAsync(user.CompanyId);

            var Date = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

            var contacts = await _context.Contacts.Where(c => exportContactDTO.contactIds.Contains(c.ContactId)).ToListAsync();
            if (contacts == null)
                return (false, "No contacts to export");

            var path = string.Empty;

            DataTable dt = new DataTable();

            dt.Clear();
            dt.Columns.Add("ContactId");
            dt.Columns.Add("FirstName");
            dt.Columns.Add("LastName");
            dt.Columns.Add("EmailOne");
            dt.Columns.Add("EmailTwo");
            dt.Columns.Add("PhoneNumber");
            dt.Columns.Add("Mobile");
            dt.Columns.Add("AddressOne");
            dt.Columns.Add("AddressTwo");

            foreach (var contact in contacts)
            {
                DataRow row = dt.NewRow();
                row["ContactId"] = contact.ContactId;
                row["FirstName"] = contact.FirstName;
                row["LastName"] = contact.LastName;
                row["EmailOne"] = contact.EmailOne;
                row["EmailTwo"] = contact.EmailTwo;
                row["PhoneNumber"] = contact.PhoneNumber;
                row["Mobile"] = contact.Mobile;
                row["AddressOne"] = contact.AddressOne;
                row["AddressTwo"] = contact.AddressTwo;

                dt.Rows.Add(row);
            }

            path = Path.Combine(_environment.WebRootPath, "Exports", $"{company!.CompanyName}_Contacts_{Date}.xlsx");
         
            try
            {
                using (XLWorkbook ewb = new XLWorkbook())
                {
                    ewb.Worksheets.Add(dt, "Contacts");
                    ewb.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                return (false, "An error occurred while creating the file. " + ex.Message);
            }

            var result = await _mailService.SendExportContactEmail(exportContactDTO.Email, path);
            if (!result)
                return (false, "An error occurred while exporting by email.");

            return (true, "Your contacts has been Export successfully."); ;
        }
        private void DeleteImage(string imageURL)
        {
            if (!string.IsNullOrEmpty(imageURL))
            {
                // Check if the file exists and delete it if it does
                if (File.Exists(imageURL))
                {
                    File.Delete(imageURL);
                }
            }
        }
        private async Task<User> GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

            return user!;
        }
        private async Task<Contact> GetContactByEmail(string email)
        {
            var contact = await _context.Contacts.SingleOrDefaultAsync(c => c.EmailOne == email || c.EmailTwo == email);
            return contact!;
        }

    }
}
