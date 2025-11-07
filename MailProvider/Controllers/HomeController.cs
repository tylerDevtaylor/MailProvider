using MailProvider.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using MailProvider.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json.Serialization;

namespace MailProvider.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _sessionContext;
        private readonly IGoogleService _googleService;

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor sessionContext, IGoogleService googleService)
        {
            _logger = logger;
            _sessionContext = sessionContext;
            _googleService = googleService;
        }

        public IActionResult Invalid()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        public async Task<IActionResult> Dashboard()
        {
            var sessionUser = _sessionContext.HttpContext?.Session.GetString("User");
            if (string.IsNullOrEmpty(sessionUser))
            {
                return View(); 
            }

            var user = JsonSerializer.Deserialize<UserAccount>(sessionUser) ?? null;
            if (user is null)
            {
                return View();
            }

            var model = await _googleService.GetMessagesAsync(user.Email).ConfigureAwait(false);

            return View("Dashboard", model);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
