using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailProvider.Models;
using Message = MailProvider.Models.Message;

namespace MailProvider.Services
{
    public class GoogleService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleService> _log;
        public GoogleService(IConfiguration configuration, ILogger<GoogleService> log)
        {
            _configuration = configuration;
            _log = log;
        }
        public async Task<(UserCredential?, string?)> GetCredentials(string email, string password)
        {
            try
            {
                string[] Scopes = [GmailService.Scope.GmailReadonly]; // Define required scopes
                string ApplicationName = "MailProvider";
                UserCredential credential;
                var codeReceiver = new LocalServerCodeReceiverAutoClose(8080);
                using (var stream = new FileStream("Credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                        Scopes,
                        "tyler.taylor.dev@gmail.com",
                        CancellationToken.None,
                        new FileDataStore("TokenStore", true),
                        codeReceiver);
                }
                if (credential is null)
                {
                    return (null, null);
                }
                return (credential, ApplicationName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IList<Message>> GetMessagesAsync(User user)
        {
            try
            {
                string[] Scopes = [GmailService.Scope.GmailReadonly]; // Define required scopes
                string ApplicationName = "MailProvider";
                UserCredential credential;
                var codeReceiver = new LocalServerCodeReceiverAutoClose(8080);
                // Load credentials from the JSON file
                using (var stream = new FileStream("Credentials.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                        Scopes,
                        user.UserId,
                        CancellationToken.None,
                        new FileDataStore(credPath, true),
                        codeReceiver).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Gmail API service
                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Fetch messages
                var request = service.Users.Messages.List("me");
                request.LabelIds = "INBOX"; // Filter for inbox messages
                request.MaxResults = 20;   // Limit to 10 messages

                var response = await request.ExecuteAsync();

                var messages = new List<Message>();
                foreach (var msg in response.Messages)
                {
                    
                    var req = service.Users.Messages.Get(user.UserId, msg.Id);
                    var rep = await req.ExecuteAsync();
                    var m = new Message()
                    {
                        Body = rep.Raw,
                        Header = rep.Snippet,
                        Date = rep.InternalDate
                    };
                    messages.Add(m);
                }

                return messages;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<Message>();
            }
        }
    }
}
