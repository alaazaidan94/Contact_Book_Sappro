
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ContactBook_Domain.Models;

public class User : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserStatus Status { get; set; } = UserStatus.Pending;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Roles Role { get; set; } = Roles.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool isDeleted { get; set; } = false;

    public int CompanyId { get; set; }
    public Company? Company { get; set; }
}

public enum UserStatus
{
    Pending = 1,
    Active = 2,
    Locked = 3
}

public enum Roles
{
    User = 1,
    Admin = 2,
    Owner = 3
}

