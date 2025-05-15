using System.Security.Claims;
using AvstickareApi.Data;
using AvstickareApi.Models;
using AvstickareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvstickareApi.Controllers
{
    //hjälper till att bygga apicontroller
    [ApiController]
    [Route("api/[controller]")]
    //tar emot och använder service och context (konstruktor), ärver från controllerbase som kan användas för apier.
    public class AuthController(AuthService authService, AvstickareContext context) : ControllerBase
    {
        private readonly AuthService _authService = authService;
        private readonly AvstickareContext _context = context;


        //registrera ny användare
        [HttpPost("registrera")]
        public async Task<ActionResult<AppUser>> Register(AppUser newUser)
        {

            //är det enligt modell?
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //ingen null- eller whitespace
            if (string.IsNullOrWhiteSpace(newUser.Email) || string.IsNullOrWhiteSpace(newUser.Password))
            {
                return BadRequest(new { message = "E-post eller lösenord krävs." });
            }

            //längd på lösenord minst 8 tecken
            if (newUser.Password.Length < 8)
            {
                return BadRequest(new { message = "Lösenordet måste vara minst 8 tecken." });
            }

            //kontrollera om e-post redan används
            if (await _context.AppUsers.AnyAsync(user => user.Email == newUser.Email))
            {
                return BadRequest(new { message = "E-postadressen används redan." });
            }

            //hasha läsenordet med bcrypt
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            //tvinga rollen till User så ingen kan sätta Admin vid registrering
            newUser.Role = "User";

            //lägg till användare
            _context.AppUsers.Add(newUser);
            await _context.SaveChangesAsync();

            //skapa token
            var token = _authService.CreateToken(newUser);

            //returnera ok och token för möjlighet att logga in direkt
            return Ok(new { message = "Användare registrerad.", token });

        }

        //logga in
        [HttpPost("logga-in")]
        public async Task<IActionResult> Login(LoginRequest loginUser)
        {
            //giltig modell?
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //e-post och lösenord korrekt ifyllt?
            if (string.IsNullOrWhiteSpace(loginUser.Email) || string.IsNullOrWhiteSpace(loginUser.Password))
            {
                return BadRequest(new { message = "E-post eller lösenord krävs." });
            }

            //hämta användare från databasen
            var user = await _context.AppUsers.FirstOrDefaultAsync(user => user.Email == loginUser.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Felaktiga inloggningsuppgifter" });
            }

            //verifiera lösenord
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Felaktiga inloggningsuppgifter" });
            }

            //skapa token
            var token = _authService.CreateToken(user);

            //returnera och token för att logga in
            return Ok(new LoginResponse
            {
                Message = "Inloggning lyckades",
                Token = token
            });
        }

        //ändra lösenord, skyddad
        [Authorize]
        [HttpPost("andra-losenord")]
        public async Task<IActionResult> ChangePassword(ChangePassword change)
        {
            //kontrollera input
            if (string.IsNullOrWhiteSpace(change.OldPassword) || string.IsNullOrWhiteSpace(change.NewPassword))
            {
                return BadRequest(new { message = "Både det gamla och det nya lösenordet måste anges." });
            }

            //lösenordet måste vara minst 8 tecken
            if (change.NewPassword.Length < 8)
            {
                return BadRequest(new { message = "Det nya lösenordet måste vara minst 8 tecken." });
            }

            //hämta användare
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "Användare kunde inte identifieras." });
            }

            //leta efter användare baserat på id
            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Användare hittades inte" });
            }

            //verifiera gamla lösenordet
            if (!BCrypt.Net.BCrypt.Verify(change.OldPassword, user.Password))
            {
                return BadRequest(new { message = "Det gamla lösenordet är felaktigt." });
            }

            //hasha och spara det nya lösenordet
            user.Password = BCrypt.Net.BCrypt.HashPassword(change.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lösenordet har uppdaterats." });
        }

        //hämta användarinfo
        [Authorize]
        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            //hämta användare
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return Unauthorized(new { message = "Användare kunde inte identifieras"});
            }

             //leta efter användare baserat på id
            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Användare hittades inte" });
            }

            //returnera användarinfo
            return Ok(new { user.UserName, user.FirstName, user.LastName, user.Email});
        }

        //route för global felhantering under produktion
        [Route("error")]
        //visas inte i swagger
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult HandleError()
        {
            var errorContext = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return Problem(
                detail: errorContext?.Error.StackTrace,
                title: errorContext?.Error.Message);
        }

    }
}