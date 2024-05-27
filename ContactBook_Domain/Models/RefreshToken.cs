using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactBook_Domain.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(100)]
        public required string Token { get; set; }
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime DateExpiresUtc { get; set; }
        public bool IsExpired => DateTime.UtcNow >= DateExpiresUtc;
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User user { get; set; }
    }
}
