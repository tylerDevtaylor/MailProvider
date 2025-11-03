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
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailController> _log;
        private readonly IPasswordService _passwordService;
        public AccountController(IConfiguration configuration, ILogger<GmailController> log, PasswordService passwordService)
        {
            _configuration = configuration;
            _log = log;
            _passwordService = passwordService;
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
                    Active = true
                };
                var connection = new SqlConnection(_configuration["ConnectionStrings:UserAccount"]);
                _ = await connection.ExecuteAsync("InsertNewUserAccount", accountRecord, commandType:      CommandType.StoredProcedure).ConfigureAwait(false);

                return View("_Layout");
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
                


                return Redirect($"../Home");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }
        }
    }
}
