using System.Data;
using System.Text.Json;
using Dapper;
using MailProvider.Interfaces;
using MailProvider.Models;
using MailProvider.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MailProvider.Controllers
{
    [Route("Account/")]
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailController> _log;
        private readonly IPasswordService _passwordService;
        private readonly IHttpContextAccessor _sessionContext;
        private readonly IGoogleService _googleService;
        
        public AccountController(IConfiguration configuration, ILogger<GmailController> log, IPasswordService passwordService,
            IHttpContextAccessor sessionContext, IGoogleService googleService)
        {
            _configuration = configuration;
            _log = log;
            _passwordService = passwordService;
            _sessionContext = sessionContext;
            _googleService = googleService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAccount(RegisterUser register)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _log.LogError("GmailController <> RegisterAccount: Invalid Model State.");
                    return BadRequest();
                }

                var userId = Guid.NewGuid().ToString();
                var hashedPassword = _passwordService.HashPassword(userId, register.Password);
                var accountRecord = new UserAccount
                {
                    Id = userId,
                    FirstName = register.FirstName,
                    LastName = register.LastName,
                    Email = register.Email,
                    PasswordHash = hashedPassword,
                    Active = true,
                    DateAdded = DateTime.Now
                };
                var connection = new SqlConnection(_configuration["ConnectionStrings:UserAccount"]);
                var recordSaved = await connection.ExecuteAsync("InsertNewUserAccount", accountRecord, commandType:CommandType.StoredProcedure).ConfigureAwait(false);
                if (recordSaved != 1)
                {
                    _log.LogError("AccountController <> RegisterAccount: User account not created: email: {email}", register.Email);
                    return View("Invalid");
                }
                var userString = JsonSerializer.Serialize(accountRecord);
                _sessionContext.HttpContext!.Session.SetString("User", userString);
                if (_sessionContext.HttpContext!.Session.GetString("User") == null)
                {
                    _log.LogError("AccountController <> RegisterAccount: User session not saved: email {email}", register.Email);
                    return RedirectToAction("Index", "Home");
                }

                var model = await _googleService.GetMessagesAsync(accountRecord.Email).ConfigureAwait(false);
                return View("Dashboard", model);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "GmailController <> RegisterAccount: Error creating account: {@account}", register);
                return View("Invalid");
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                await using var connection = new SqlConnection(_configuration["ConnectionStrings:UserAccount"]);
                var user = await connection.QuerySingleOrDefaultAsync<UserAccount>("GetUserAccount",
                    new { @Email = email }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

                if (user == null)
                {
                    _log.LogWarning("AccountController <> Login: No account matching email: {email}", email);
                    return View("Invalid");
                }

                var passwordMatch = _passwordService.ValidateHashedPassword(user.Id, user.PasswordHash, password);
                if (!passwordMatch)
                {
                    _log.LogWarning("AccountController <> Login: Invalid password for email: {email}", email);
                    return View("Invalid");
                }

                var userString = JsonSerializer.Serialize(user);
                _sessionContext.HttpContext!.Session.SetString("User", userString);
                
                var messages = await _googleService.GetMessagesAsync(user.Email).ConfigureAwait(false);
                var dashboardViewModel = new DashboardViewModel
                {
                    Messages = messages
                };
                return View("Dashboard", dashboardViewModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }
        }

        [Route("Logout")]
        public IActionResult Logout()
        {
            try
            {
                _sessionContext.HttpContext?.Session.Remove("User");
                return Redirect("/");
            }
            catch (Exception e)
            {
                _log.LogError(e, "AccountController <> Logout: Error trying to logout user: ");
                return View("Error");
            }
        }
    }
}
