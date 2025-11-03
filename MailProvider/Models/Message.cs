namespace MailProvider.Models
{
    public class Message
    {
        public string Id { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public long? Date { get; set; }
        public string Html { get; set; }

    }
}
