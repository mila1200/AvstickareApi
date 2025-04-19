using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvstickareApi.Data;
using AvstickareApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

//lägg till, ta bort stopp i en sparad resa

namespace AvstickareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TripStopController : ControllerBase
    {
        private readonly AvstickareContext _context;

        public TripStopController(AvstickareContext context)
        {
            _context = context;
        }

        // GET: api/TripStop/trip/tripId (för att visa stoppen på en specifik resa)
        //hämtar alla stopp för specifik resa
        [HttpGet("/trip/{tripId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTripStopForTrip(int tripId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kontrollera att resan existerar och tillhör användaren
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.AppUserId == userId);

            if (trip == null)
            {
                return NotFound(new { message = "Resan hittades inte." });
            }

            //hämta stopp
            var stops = await _context.TripStops
                .Where(s => s.TripId == tripId)
                .Include(s => s.Place)
                .OrderBy(s => s.Order)
                .ToListAsync();

            if (stops == null || !stops.Any())
            {
                return NotFound(new { message = "Inga stopp hittades för denna resa." });
            }

            //returnera resultat
            var result = stops
                .Where(s => s.Place != null)
                .Select(s => new
                {
                    s.TripStopId,
                    s.Order,
                    s.Place!.PlaceId,
                    s.Place.MapServicePlaceId,
                    s.Place.Name,
                    s.Place.Lat,
                    s.Place.Lng
                });

            return Ok(result);
        }

        // POST: api/TripStop
        //lägg till nytt stopp till befintlig resa
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> PostTripStop([FromBody] TripStop tripStop)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //kontrollera att resan tillhör användaren
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.TripId == tripStop.TripId && t.AppUserId == userId);

            if (trip == null)
            {
                return BadRequest("Resan hittades inte eller tillhör inte dig.");
            }

            //kontrollera att platsen finns
            var placeExists = await _context.Places.AnyAsync(p => p.PlaceId == tripStop.PlaceId);
            if (!placeExists)
            {
                return BadRequest("Platsen hittades inte.");
            }

            //lägg till stoppet
            _context.TripStops.Add(tripStop);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Stoppet har lagts till.", tripStop.TripStopId });
        }

        // DELETE: api/TripStop/5
        //tar bort stopp från en resa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTripStop(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var stop = await _context.TripStops
                .Include(s => s.Trip)
                .FirstOrDefaultAsync(s => s.TripStopId == id);

            //om stoppet inte finns 
            if (stop == null)
            {
                return NotFound(new { message = "Stoppet hittades inte." });
            }

            //om relationen till Trip inte funkar
            if (stop.Trip == null)
            {
                return BadRequest(new { message = "Stoppet är inte kopplat till någon resa." });
            }

            //om resan inte tillhör inloggad användare
            if (stop.Trip.AppUserId != userId)
            {
                return Unauthorized(new { message = "Du har inte tillåtelse att ta bort detta stopp." });
            }

            //ta bort stoppet
            _context.TripStops.Remove(stop);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Stoppet har tagits bort från resan." });
        }
    }
}
