using AutoMapper;
using Azure.Core;
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Services.DTOs.Contact;
using ContactBook_Services.DTOs.Logs;
using ContactBook_Services.DTOs.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public ContactService(
            ContactBookContext context,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            LogService logService,
            MailService mailService,
            IMapper mapper)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logService = logService;
            _mailService = mailService;
            _mapper = mapper;
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
        public async Task<(bool,string)> AddAsync(AddContatctDTO addContatctDTO)
        {
            bool result = false;
            string message = "";

            var user = await GetCurrentUser();

            if (user == null)
            {
                message = "User Not Found";
                return (result, message);
            }

            var companyId = user.CompanyId;

            (result,message) = await UploadImage(addContatctDTO.UploadImage);

            if (!result)
                return (result, message);

            var contact = _mapper.Map<AddContatctDTO, Contact>(addContatctDTO, opts =>
            {
                opts.AfterMap((src, dest) =>
                {
                    dest.ImageUrl = "~/wwwroot/Uplodes/Contact-image";
                    dest.ImageName = message;
                    dest.CompanyId = companyId;
                });
            });

            var state = await _context.Contacts.AddAsync(contact);
            if (state.State is EntityState.Added)
            {
                await _context.SaveChangesAsync();

                var log = new LogModel()
                {
                    ContactName = contact.FirstName + " " + contact.LastName,
                    Action = LogAction.Add,
                    ActionBy = user.FirstName + " " + user.LastName,
                };

                await _logService.AddLog(log);

                message = "Create contact success";
                result = true;

                return (result, message);
            }
            return (result, message);
        }
        public async Task<bool> UpdateAsync(Contact contact)
        {
            var user = await GetCurrentUser();

            if (contact == null)
                return false;

            var state = _context.Contacts.Update(contact);

            if (state.State is EntityState.Modified)
            {
                await _context.SaveChangesAsync();

                var log = new LogModel()
                {
                    ContactName = contact.FirstName + " " + contact.LastName,
                    Action = LogAction.Update,
                    ActionBy = user.FirstName + " " + user.LastName,
                };

                await _logService.AddLog(log);

                return true;
            }
            return false;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetCurrentUser();

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.ContactId == id);
            if (contact == null)
                return false;

            var state = _context.Remove(contact);

            if (state.State is EntityState.Deleted)
            {
                await _context.SaveChangesAsync();

                var log = new LogModel()
                {
                    ContactName = contact.FirstName + " " + contact.LastName,
                    Action = LogAction.Delete,
                    ActionBy = user.FirstName + " " + user.LastName,
                };
                await _logService.AddLog(log);

                return true;
            }

            return false;
        }
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var contact = await _context.Contacts.SingleOrDefaultAsync(c => c.ContactId == id);

            if (contact == null)
                return false;

            contact.isDeleted = true;

            var state = _context.Update(contact);

            if (state.State is EntityState.Modified)
            {
                await _context.SaveChangesAsync();


                return true;
            }

            return false;
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
        public async Task<(bool, string)> UploadImage(IFormFile image)
        {
            var result = false;
            var message = "";

            List<string> validExtention = new List<string>() { ".jpg", ".png" };

            string extention = Path.GetExtension(image.FileName);

            if (!validExtention.Contains(extention))
            {
                message = $"Extention is not valid ({string.Join(',', validExtention)})";
                return (result, message);
            }

            long size = image.Length;
            if (size > (5 * 1024 * 1024))
            {
                message = "Maximum size can be 5MB";
                return (result, message);
            }

            string imageName = Guid.NewGuid().ToString() + extention;


            string path = GetPath();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string imagePath = path + "\\" + imageName;

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }

            using (FileStream stream = File.Create(imagePath))
            {
                await image.CopyToAsync(stream);

                message = "Image Upload Success";
                result = true;
            }

            return (result,message);
        } 
        private string GetPath()
        {
            return _environment.WebRootPath + "\\Uplodes\\Contact-Image\\";
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
