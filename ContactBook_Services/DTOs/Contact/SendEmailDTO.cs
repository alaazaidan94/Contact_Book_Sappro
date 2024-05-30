namespace ContactBook_Services.DTOs.Contact
{
    public class SendEmailDTO
    {
        public required string To { get; set; }
        public string? CC { get; set; }
        public string? BCC { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }


    }
}
