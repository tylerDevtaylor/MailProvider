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
        
        public AccountController(IConfiguration configuration, ILogger<GmailController> log, IPasswordService passwordService,
            IHttpContextAccessor sessionContext)
        {
            _configuration = configuration;
            _log = log;
            _passwordService = passwordService;
            _sessionContext = sessionContext;
        }
        [HttpPost("Register")]
        public async Task<ActionResult> RegisterAccount(RegisterUser register)
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
                _ = await connection.ExecuteAsync("InsertNewUserAccount", accountRecord, commandType:CommandType.StoredProcedure).ConfigureAwait(false);
                
                var user = JsonSerializer.Serialize(accountRecord);
                _sessionContext.HttpContext!.Session.SetString("User", user);
                return View($"../Home");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "GmailController <> RegisterAccount: Error creating account: {@account}", register);
                return View("Invalid");
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(string email, string password)
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


                return Redirect($"../Dashboard");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }
        }
    }
}
