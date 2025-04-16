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
                var (polyline, distance, duration) = await _mapsService.CreateTrip(trip);

                return Ok(new { Trip = trip, Polyline = polyline, Distance = distance, Duration = duration });
            
            } catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message});
            }
        }

        // POST: api/Trip (spara resa om inloggad)
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Trip>> PostTrip(Trip trip)
        {
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
