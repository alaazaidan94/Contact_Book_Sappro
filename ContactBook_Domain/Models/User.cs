
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ContactBook_Domain.Models;

public class User : IdentityUser
{
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [MaxLength(100)]
    public required string LastName { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Pending;
    public Roles Role { get; set; } = Roles.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool isDeleted { get; set; } = false;

    public int CompanyId { get; set; }
    public Company? Company { get; set; }
}

public enum UserStatus
{
    Pending,
    Active,
    Locked
}

public enum Roles
{
    [Display(Name = "User")]
    User,

    [Display(Name = "Admin")]
    Admin,
}

