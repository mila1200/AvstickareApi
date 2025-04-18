using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvstickareApi.Data;
using AvstickareApi.Models;
using AvstickareApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AvstickareApi.Controllers
{
    //generera resa, spara resa, hämta, ta bort

    [Route("api/[controller]")]
    [ApiController]
    public class TripController(AvstickareContext context, GoogleMapsService mapsService) : ControllerBase
    {
        private readonly AvstickareContext _context = context;
        private readonly GoogleMapsService _mapsService = mapsService;

        // GET: api/Trip
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
        {
            return await _context.Trips.ToListAsync();
        }

        // GET: api/Trip/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Trip>> GetTrip(int id)
        {
            var trip = await _context.Trips.FindAsync(id);

            if (trip == null)
            {
                return NotFound();
            }

            return trip;
        }


        //POST: api/Trip/plan
        //try/catch då jag hämtar info från externa källor
        [HttpPost("plan")]
        public async Task<IActionResult> PlanTrip(Trip trip)
        {
            try
            {
                //hämta rutt och koordinater
                var (polyline, distance, duration) = await _mapsService.CreateTrip(trip);

                //generera platser från polyline, skapar ej trip eller tripstop.
                var places = await _mapsService.CreatePlace(polyline!);

                return Ok(new { Trip = trip, Polyline = polyline, Distance = distance, Duration = duration, SuggestedPlaces = places });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // POST: api/Trip (spara resa om inloggad)
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Trip>> SaveTrip(Trip trip)
        {
            trip.AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(trip.AppUserId))
            {
                return BadRequest("Användare måste vara inloggad för att spara resan");
            }

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTrip", new { id = trip.TripId }, trip);
        }

        // DELETE: api/Trip/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //kan eventuellt användas för att kolla om resor finns när tripstops ska uppdateras
        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.TripId == id);
        }
    }
}
