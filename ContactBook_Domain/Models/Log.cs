namespace ContactBook_Domain.Models
{
    public class Log
    {
        public int LogId { get; set; }
        public string ContactName { get; set; }
        public DateOnly CreatedAt { get; set; } = new DateOnly();
        public string Action { get; set; }
        public string ActionBy { get; set; }

    }
}
