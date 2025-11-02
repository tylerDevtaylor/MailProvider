using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using MailProvider.Services;
using System.Text.Json;
using MailProvider.Models;

namespace MailProvider.Controllers
{
    [Route("GmailController/")]
    public class GmailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailController> _log;
        private readonly IHttpContextAccessor _sessionContext;
        private readonly GoogleService _googleService;

        public GmailController(IConfiguration configuration, ILogger<GmailController> log, IHttpContextAccessor sessionContext, GoogleService googleService)
        {
            _configuration = configuration;
            _log = log;
            _sessionContext = sessionContext;
            _googleService = googleService;
        }


        [HttpPost("Login")]
        public async Task<ActionResult> Login(string email, string password)
        {
            try
            {
                if (email != "tyler.taylor.dev@gmail.com" || password != "test1")
                {
                    return View("Invalid");
                }
                
                var response = await _googleService.GetCredentials(email, password).ConfigureAwait(false);
                if (response.Item1 is null)
                {
                    Console.WriteLine($"Null Google User Credential. email: {email}.");
                    return View("Invalid");
                }

                var session = _sessionContext.HttpContext!.Session;
                var user = new User()
                {
                    UserId = response.Item1.UserId,
                    Token = new TokenResponse()
                    {
                        AccessToken = response.Item1.Token.AccessToken,
                        RefreshToken = response.Item1.Token.RefreshToken,
                        TokenType = response.Item1.Token.TokenType
                    }
                };
                var serializedUser = JsonSerializer.Serialize(response.Item1);
                session.SetString("User", serializedUser);
                
                return Redirect($"../Home");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }
        }

        [HttpGet("Dashboard")]
        public async Task<ActionResult> GetDashboard()
        {
            try
            {
                var sessionUser = _sessionContext.HttpContext!.Session.GetString("User");
                
                if (sessionUser is null)
                {
                    return View("Invalid");
                }

                var user = JsonSerializer.Deserialize<User>(sessionUser);

                if (user is null)
                {
                    return View("Invalid");
                }
                var response = await _googleService.GetMessagesAsync(user).ConfigureAwait(false);
                return View("Dashboard", response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
    }
}
