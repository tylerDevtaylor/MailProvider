namespace MailProvider.Models
{
    public class DashboardViewModel
    {
        public IList<Message> Messages { get; set; }
        public Compose Compose { get; set; }
    }
}
