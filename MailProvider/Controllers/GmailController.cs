using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using MailProvider.Services;
using System.Text.Json;
using MailProvider.Interfaces;
using MailProvider.Models;

namespace MailProvider.Controllers
{
    [Route("GmailController/")]
    public class GmailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailController> _log;
        private readonly IHttpContextAccessor _sessionContext;
        private readonly IGoogleService _googleService;
        public GmailController(IConfiguration configuration, ILogger<GmailController> log, IHttpContextAccessor sessionContext, 
            IGoogleService googleService)
        {
            _configuration = configuration;
            _log = log;
            _sessionContext = sessionContext;
            _googleService = googleService;
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

                var user = JsonSerializer.Deserialize<UserAccount>(sessionUser);

                if (user is null)
                {
                    return View("Invalid");
                }
                var response = await _googleService.GetMessagesAsync(user.Email).ConfigureAwait(false);
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
