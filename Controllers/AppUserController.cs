using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvstickareApi.Data;
using AvstickareApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace AvstickareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //endast för admin, att hämta användare, uppdatera användare och ta bort användare
    [Authorize(Roles = "Admin")]
    public class AppUserController(AvstickareContext context) : ControllerBase
    {
        private readonly AvstickareContext _context = context;

        // GET: api/AppUser
        [HttpGet]
        //anonymt objekt istället för AppUser för att inte Password ska skickas med
        public async Task<ActionResult<IEnumerable<object>>> GetAppUsers()
        {
            //returnerar en användare, men sorterar ut lösenordet. 
            var user = await _context.AppUsers
            .Select(u => new
            {
                u.AppUserId,
                u.UserName,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.CreatedAt
            })
             .ToListAsync();

            return Ok(user);
        }

        // GET: api/AppUser/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAppUser(string id)
        {
            var user = await _context.AppUsers.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.AppUserId,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.CreatedAt
            });
        }

        // PUT: api/AppUser/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppUser(string id, AppUser updated)
        {
            if (id != updated.AppUserId)
            {
                return BadRequest();
            }

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            // Uppdatera tillåtna fält
            user.UserName = updated.UserName;
            user.FirstName = updated.FirstName;
            user.LastName = updated.LastName;
            user.Role = updated.Role;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/AppUser/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppUser(string id)
        {
            var appUser = await _context.AppUsers.FindAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }

            _context.AppUsers.Remove(appUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
