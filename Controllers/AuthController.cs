using AvstickareApi.Data;
using AvstickareApi.Models;
using AvstickareApi.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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


        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(AppUser newUser)
        {
            //kontrollerar modell
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //kontrollera om e-post redan används
            if (_context.AppUsers.Any(user => user.Email == newUser.Email))
            {
                return BadRequest(new { message = "Email används redan." });
            }

            //hasha läsenordet med bcrypt
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash);

            //lägg till användare
            _context.AppUsers.Add(newUser);
            await _context.SaveChangesAsync();

            //returnera ok
            return Ok(new { message = "Användare registrerad." });

        }

        //route för global felhantering under produktion
        [Route("error")]
        //visas inte i swagger
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult HandleError()
        {
            var errorContext = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return Problem (
                detail: errorContext?.Error.StackTrace,
                title: errorContext?.Error.Message);
        }

    }
}