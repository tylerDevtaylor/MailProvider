using System.ComponentModel;

namespace MailProvider.Models
{
    public class LoginModel
    {
        [DisplayName("Email")]
        public string Email { get; set; } = string.Empty;
        [DisplayName("Password")]
        public string Password { get; set; } = string.Empty;
    }
}
