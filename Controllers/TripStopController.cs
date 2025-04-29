using AvstickareApi.Data;
using AvstickareApi.Models;
using AvstickareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AvstickareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripStopController(AvstickareContext context, PlaceService placeService) : ControllerBase
{
    private readonly AvstickareContext _context = context;
    private readonly PlaceService _placeService = placeService;

    
    //Hämtar alla stopp för en viss resa.
    [Authorize]
    [HttpGet("{tripId}")]
    public async Task<IActionResult> GetTripStops(int tripId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var trip = await _context.Trips
            .Include(t => t.TripStops)
            .FirstOrDefaultAsync(t => t.TripId == tripId && t.AppUserId == userId);

        if (trip == null)
        {
            return NotFound(new { message = "Resan hittades inte." });
        }
    
        var stops = trip.TripStops?.Select(s => new
        {
            s.TripStopId,
            s.MapServicePlaceId,
            s.Order
        });

        return Ok(stops);
    }

   
    //lägger till ett nytt stopp till en resa.
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddTripStop([FromBody] TripStop tripStop)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == tripStop.TripId && t.AppUserId == userId);

        if (trip == null)
        {
            return NotFound(new { message = "Resan hittades inte." });
        }
        
        if (string.IsNullOrWhiteSpace(tripStop.MapServicePlaceId))
        {
             return BadRequest("Ogiltigt plats-ID.");
        }
        
        await _placeService.EnsurePlaceExists(tripStop.MapServicePlaceId);

        _context.TripStops.Add(tripStop);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTripStops), new { tripId = tripStop.TripId }, tripStop);
    }

    //Tar bort ett stopp från en resa.
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTripStop(int id)
    {
        var tripStop = await _context.TripStops.FindAsync(id);

        if (tripStop == null)
        {
            return NotFound(new { message = "Stoppet hittades inte." });
        }
            
        _context.TripStops.Remove(tripStop);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Stoppet har tagits bort." });
    }
}