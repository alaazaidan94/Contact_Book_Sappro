using ContactBook_Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContactBook_Infrastructure.DBContexts
{
    public class ContactBookContext : IdentityDbContext<User>
    {
        public ContactBookContext(DbContextOptions<ContactBookContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ContactBook_DB;Integrated Security=True;Trust Server Certificate=True;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
