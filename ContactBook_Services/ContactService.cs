using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace ContactBook_Services
{
    public class ContactService
    {
        private readonly ContactBookContext _context;

        public ContactService(ContactBookContext context)
        {
            _context = context;
        }

        public async Task<List<Contact>> GetAllAsync()
        {
            return await _context.Contacts.ToListAsync();
        }
        public async Task<Contact> GetByIdAsync(int id)
        {
            return await _context.Contacts.FirstOrDefaultAsync(p => p.ContactId == id);

        }
        public async Task<bool> AddAsync(Contact entity)
        {
            var state = await _context.Contacts.AddAsync(entity);

            if (state.State is EntityState.Added)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> UpdateAsync(Contact contact)
        {
            //var contact = await _context.Contacts.SingleOrDefaultAsync(c => c.ContactId == contactId);

            if (contact == null)
                return false;

            var state = _context.Contacts.Update(contact);

            if (state.State is EntityState.Modified)
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.ContactId == id);
            if (contact == null)
                return false;

            var state = _context.Remove(contact);

            if (state.State is EntityState.Deleted)
            {
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
