using ContactBook_Domain.Models;
using ContactBook_Infrastructure.DBContexts;
using ContactBook_Infrastructure.InitialData;
using ContactBook_Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ContactBookContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add services to the container.

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<MailService>();

builder.Services.AddScoped<ContextSeedService>();

builder.Services.AddScoped<CompanyService>();

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<ContactService>();

builder.Services.AddScoped<LogService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


// defining our IdentityCore Service
builder.Services.AddIdentityCore<User>(options =>
{
    // password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;

    // for email confirmation
    //options.SignIn.RequireConfirmedEmail = false;


    // for email Unique
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>() 
    .AddRoleManager<RoleManager<IdentityRole>>() 
    .AddEntityFrameworkStores<ContactBookContext>() 
    .AddSignInManager<SignInManager<User>>() 
    .AddUserManager<UserManager<User>>() 
    .AddDefaultTokenProviders();

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
           
            ValidIssuer = builder.Configuration["JWT:Issuer"],

            ValidateIssuer = true,
            
            ValidateAudience = false,
            
            ValidateLifetime = true,
           
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#region ContextSeed
using var scope = app.Services.CreateScope();
try
{
    var contextSeedService = scope.ServiceProvider.GetService<ContextSeedService>();
    await contextSeedService!.InitializeContextAsync();
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    logger!.LogError(ex.Message, "Failed to initialize and seed the database");
}
#endregion

app.Run();
