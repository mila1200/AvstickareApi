using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvstickareApi.Data;
using AvstickareApi.Models;

//lägg till, ta bort stopp i en sparad resa

namespace AvstickareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripStopController : ControllerBase
    {
        private readonly AvstickareContext _context;

        public TripStopController(AvstickareContext context)
        {
            _context = context;
        }

        // GET: api/TripStop/trip/tripId (för att visa stoppen på en specifik resa)
        [HttpGet("/trip/{tripId}")]
        public async Task<ActionResult<IEnumerable<TripStop>>> GetTripStopForTrip(int tripId)
        {
            var stops = await _context.TripStops
            .Where(tripstop => tripstop.TripId == tripId)
            .Include(tripstop => tripstop.Place)
            .OrderBy(tripstop => tripstop.Order)
            .ToListAsync();

            if (stops == null)
            {
                return NotFound();
            }

            return stops;
        }

        // POST: api/TripStop
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TripStop>> PostTripStop(TripStop tripStop)
        {
            _context.TripStops.Add(tripStop);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTripStop", new { id = tripStop.TripStopId }, tripStop);
        }

        // DELETE: api/TripStop/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTripStop(int id)
        {
            var tripStop = await _context.TripStops.FindAsync(id);
            if (tripStop == null)
            {
                return NotFound();
            }

            _context.TripStops.Remove(tripStop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TripStopExists(int id)
        {
            return _context.TripStops.Any(e => e.TripStopId == id);
        }
    }
}
