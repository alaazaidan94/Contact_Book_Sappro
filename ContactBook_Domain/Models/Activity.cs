namespace ContactBook_Domain.Models
{
    public class Activity
    {
        public int ActivityId { get; set; }
        public DateOnly CreatedAt { get; set; } = new DateOnly();
        public int Action { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int ContactId { get; set; }
        public Contact Contact { get; set; }
    }
}
