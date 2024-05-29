namespace ContactBook_Services.DTOs.Contact
{
    public class SendEmailDTO
    {
        public string ToEmail { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }


    }
}
