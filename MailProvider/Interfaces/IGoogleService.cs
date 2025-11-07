using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using MailProvider.Models;

namespace MailProvider.Interfaces
{
    public interface IGoogleService
    {
        public Task<GmailService> GetCredentials(string email);
        public Task<IList<Message>> GetMessagesAsync(string email);
    }
}
