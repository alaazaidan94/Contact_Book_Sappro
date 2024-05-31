using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ContactBook_Domain.Models
{
    public class Log
    {
        public int LogId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ContactName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogAction Action { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ActionBy { get; set; }

        public int CompanyId { get; set; }

    }

    public enum LogAction
    {
        Access = 1,
        Add = 2,
        Update = 3,
        Delete = 4,
        SoftDelete = 5,
        EmailSent = 6
    }
}
