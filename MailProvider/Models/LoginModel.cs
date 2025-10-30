using System.ComponentModel;

namespace MailProvider.Models
{
    public class LoginModel
    {
        [DisplayName("Username")]
        public string Username { get; set; }
        [DisplayName("Password")]
        public string Password { get; set; }
    }
}
