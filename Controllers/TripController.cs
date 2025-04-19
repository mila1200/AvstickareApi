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
    [Authorize]
    public class TripController(GoogleMapsService mapsService, AvstickareContext context) : ControllerBase
    {

        private readonly GoogleMapsService _mapsService = mapsService;
        private readonly AvstickareContext _context = context;

        // GET: api/Trip
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrips()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var trips = await _context.Trips
                .Where(t => t.AppUserId == userId)
                .Include(t => t.FromPlace)
                .Include(t => t.ToPlace)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TripId,
                    t.Name,
                    From = t.FromPlace != null ? t.FromPlace.Name : null,
                    To = t.ToPlace != null ? t.ToPlace.Name : null,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(trips);
        }

        // GET: api/Trip/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrip(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var trip = await _context.Trips
                .Include(t => t.FromPlace)
                .Include(t => t.ToPlace)
                .FirstOrDefaultAsync(t => t.TripId == id && t.AppUserId == userId);

            if (trip == null)
            {
                return NotFound(new { message = "Resan hittades inte." });
            }

            return Ok(new
            {
                trip.TripId,
                trip.Name,
                trip.CreatedAt,
                From = new { trip.FromPlace?.PlaceId, trip.FromPlace?.Name },
                To = new { trip.ToPlace?.PlaceId, trip.ToPlace?.Name }
            });
        }

        //POST: api/Trip/plan
        //try/catch då jag hämtar info från externa källor
        [HttpPost("plan")]
        public async Task<IActionResult> PlanTrip(Trip trip)
        {
            try
            {
                //hämta rutt och koordinater
                var (polyline, distance, duration) = await _mapsService.CreateTrip(trip.FromPlaceId, trip.ToPlaceId);

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
        [HttpPost]
        public async Task<ActionResult<Trip>> SaveTrip(Trip trip)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            // kontrollera att platserna finns
            var fromExists = await _context.Places.AnyAsync(p => p.PlaceId == trip.FromPlaceId);
            var toExists = await _context.Places.AnyAsync(p => p.PlaceId == trip.ToPlaceId);

            if (!fromExists || !toExists)
            {
                return BadRequest("Start- eller slutplatsen finns inte.");
            }

            trip.AppUserId = userId;
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrip), new { id = trip.TripId }, trip);
        }

        // DELETE: api/Trip/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.TripId == id && t.AppUserId == userId);

            if (trip == null)
            {
                return NotFound(new { message = "Resan hittades inte." });
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resan har tagits bort." });
        }
    }
}
