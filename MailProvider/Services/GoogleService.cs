using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using MailProvider.Models;
using System.Text;
using MailProvider.Interfaces;
using Message = MailProvider.Models.Message;

namespace MailProvider.Services
{
    public class GoogleService : IGoogleService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleService> _log;
        public GoogleService(IConfiguration configuration, ILogger<GoogleService> log)
        {
            _configuration = configuration;
            _log = log;
        }
        public async Task<GmailService> GetCredentials(string email)
        {
            try
            {
                UserCredential credential;
                var codeReceiver = new LocalServerCodeReceiverAutoClose(8080);
                using (var stream = new FileStream("Credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                        [GmailService.Scope.GmailReadonly],
                        email,
                        CancellationToken.None,
                        new FileDataStore("TokenStore", true),
                        codeReceiver);
                }

                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MailProvider"

                });

                return service;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IList<Message>> GetMessagesAsync(string email)
        {
            try
            {

                var service = await GetCredentials(email).ConfigureAwait(false);

                // Fetch messages
                var request = service.Users.Messages.List("me");
                request.LabelIds = "INBOX"; // Filter for inbox messages
                request.MaxResults = 20;   // Limit to 20 messages
                var response = await request.ExecuteAsync();

                var messages = new List<Message>();
                foreach (var msg in response.Messages)
                {
                    
                    var req = service.Users.Messages.Get("me", msg.Id);
                    var rep = await req.ExecuteAsync();
                    var htmlPart = "";
                    if (msg.Payload is not null)
                    {
                        htmlPart = GetHtmlPart(msg.Payload);
                    }

                    var m = new Message()
                    {
                        Id = rep.Id,
                        Body = rep.Raw,
                        Header = rep.Snippet,
                        Date = rep.InternalDate,
                        Html = htmlPart
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


        private string GetHtmlPart(MessagePart payload)
        {
            if (payload == null)
                return string.Empty;

            // Check if this part is HTML
            if (payload.MimeType == "text/html" && payload.Body?.Data != null)
            {
                return DecodeBase64Url(payload.Body.Data);
            }

            // Check if it's multipart
            if (payload.Parts != null)
            {
                // First try to find text/html directly
                foreach (var part in payload.Parts)
                {
                    if (part.MimeType == "text/html" && part.Body?.Data != null)
                    {
                        return DecodeBase64Url(part.Body.Data);
                    }
                }

                // If not found, check nested multipart (like multipart/alternative)
                foreach (var part in payload.Parts)
                {
                    if (part.MimeType?.StartsWith("multipart/") == true)
                    {
                        var html = GetHtmlPart(part);
                        if (!string.IsNullOrEmpty(html))
                            return html;
                    }
                }

                // Fallback to plain text if no HTML found
                foreach (var part in payload.Parts)
                {
                    if (part.MimeType == "text/plain" && part.Body?.Data != null)
                    {
                        return DecodeBase64Url(part.Body.Data);
                    }
                }
            }

            return string.Empty;
        }

        private string DecodeBase64Url(string base64Url)
        {
            if (string.IsNullOrEmpty(base64Url))
                return string.Empty;

            // Gmail uses URL-safe base64 encoding, so we need to convert it to standard base64
            string base64 = base64Url.Replace('-', '+').Replace('_', '/');

            // Add padding if needed
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            try
            {
                byte[] data = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return string.Empty;
            }
        }

    }
}
