using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvstickareApi.Data;
using AvstickareApi.Models;

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

        // GET: api/TripStop
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripStop>>> GetTripStops()
        {
            return await _context.TripStops.ToListAsync();
        }

        // GET: api/TripStop/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TripStop>> GetTripStop(int id)
        {
            var tripStop = await _context.TripStops.FindAsync(id);

            if (tripStop == null)
            {
                return NotFound();
            }

            return tripStop;
        }

        // PUT: api/TripStop/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTripStop(int id, TripStop tripStop)
        {
            if (id != tripStop.TripStopId)
            {
                return BadRequest();
            }

            _context.Entry(tripStop).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TripStopExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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
