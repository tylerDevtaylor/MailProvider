using Microsoft.AspNetCore.Mvc;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using MailProvider.Models;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using System.Net;

namespace MailProvider.Controllers
{
    [Route("GmailController/")]
    public class GmailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailController> _log;
        public GmailController(IConfiguration configuration, ILogger<GmailController> log)
        {
            _configuration = configuration;
            _log = log;
        }


        [HttpPost("Login")]
        public async Task<ActionResult> Login(string username, string password)
        {
            try
            {
                if (username != "tyler.taylor.dev@gmail.com" || password != "test1")
                {
                    return View("Invalid");
                }

                return Redirect($"../Home/Privacy");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }
        }
        private async Task<(UserCredential, string)> GetCredentials()
        {
            try
            {
                string[] Scopes = [GmailService.Scope.GmailReadonly]; // Define required scopes
                string ApplicationName = "MailProvider";
                UserCredential credential;
                
                using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "Credentials.json";
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                return (credential, ApplicationName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task<ActionResult<BaseResponse<dynamic>>> Authenticate(string email, string password)
        {
            try
            {
                var creds = await GetCredentials();
                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = creds.Item1,
                    ApplicationName = creds.Item2,
                });
                var messages = service.Users.Messages.List("me");
                return new BaseResponse<dynamic>(true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "{service} {controller}: {eventName} Internal Server Error.", "MailProvider", "GmailController", "Authenticate");
                return StatusCode(500, new BaseResponse<ErrorResponse>(new ErrorResponse(500, "Internal Server Error")));
            }
        }
        
    }
}
