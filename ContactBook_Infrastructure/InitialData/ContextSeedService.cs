
using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContactBook_Infrastructure.InitialData
{
    public class ContextSeedService
    {
        private readonly ContactBookContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ContextSeedService(ContactBookContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeContextAsync()
        {
            if (_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Count() > 0)
            {
                // applies any pending migration into our database
                await _context.Database.MigrateAsync();
            }

            if (!_context.Companies.Any())
            {
                var Company = new Company()
                {
                    CompanyName = "TestCompany",
                    VatNumber = "112540",
                    City = "Azaz",
                    Country = "Syria",
                    State = "state",
                    StreetOne = "AAA",
                    StreetTwo = "BBB",
                    Zip = "12345"
                };
                await _context.Companies.AddAsync(Company);
                await _context.SaveChangesAsync();
            }

            if (!_userManager.Users.Any())
            {
                var Admin = new User()
                {
                    FirstName = "Alaa",
                    LastName = "Zaidan",
                    Email = "alaa.zydan94@gmail.com",
                    UserName = "alaa.zydan94@gmail.com",
                    EmailConfirmed = true,
                    Role = Roles.Admin,
                    CompanyId = 1
                };
                await _userManager.CreateAsync(Admin, "112233");
            }
        }
    }
}
