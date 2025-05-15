using System.Security.Claims;
using AvstickareApi.Data;
using AvstickareApi.Models;
using AvstickareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvstickareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    //databasen
    private readonly AvstickareContext _context;
    private readonly PlaceService _placeService;
    private readonly RouteService _routeService;
    private readonly SuggestedPlaceService _suggestedPlaceService;

    public TripController(AvstickareContext context, PlaceService placeService, RouteService routeService, SuggestedPlaceService suggestedPlaceService)
    {
        _context = context;
        _placeService = placeService;
        _routeService = routeService;
        _suggestedPlaceService = suggestedPlaceService;
    }

    // POST: api/Trip/plan
    //för att generera en rutt mellan två orter
    //hämtar PlaceId från text, koordinater via Google, rutt, och föreslagna platser längs vägen
    [HttpPost("plan")]
    public async Task<IActionResult> PlanTrip([FromBody] PlanTripRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FromLocation) || string.IsNullOrWhiteSpace(request.ToLocation))
            {
                return BadRequest("Start- och slutdestination måste anges.");
            }

            //hämta Google PlaceId
            var fromPlaceId = await _placeService.GetPlaceIdFromLocation(request.FromLocation);
            var toPlaceId = await _placeService.GetPlaceIdFromLocation(request.ToLocation);

            //kontrollera om PlaceId finns i databasen
            await _placeService.EnsurePlaceExists(fromPlaceId);
            await _placeService.EnsurePlaceExists(toPlaceId);

            //hämta koordinater för rutt
            var fromCoords = await _placeService.GetCoordinates(fromPlaceId);
            var toCoords = await _placeService.GetCoordinates(toPlaceId);

            //skapa rutt från A till B
            var (polyline, distance, duration) = await _routeService.CreateTripRoute(fromCoords, toCoords);

            //hämta föreslagna platser längs rutten
            var suggestedPlaces = await _suggestedPlaceService.GetSuggestedPlacesAlongRoute(polyline!);

            //för att matcha platsinfo i frontend
            var frontendPlaces = suggestedPlaces.Select(places => new
            {
                Name = places.Name,
                Lat = places.Latitude,
                Lng = places.Longitude,
                MapServicePlaceId = places.Id
            });

            return Ok(new
            {
                FromPlaceId = fromPlaceId,
                ToPlaceId = toPlaceId,
                Polyline = polyline,
                Distance = distance,
                Duration = duration,
                SuggestedPlaces = frontendPlaces
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/Trip
    //returnerar alla sparade resor för inloggad användare
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetTrips()
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
                From = t.FromPlace != null ? t.FromPlace.MapServicePlaceId : null,
                To = t.ToPlace != null ? t.ToPlace.MapServicePlaceId : null,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(trips);
    }

    // GET: api/Trip/{id}
    //returnerar en specifik sparad resa för inloggad användare
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrip(int id)
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
            From = trip.FromPlace?.MapServicePlaceId,
            To = trip.ToPlace?.MapServicePlaceId
        });
    }

    // POST: api/Trip
    //sparar en ny resa för inloggad användare
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SaveTrip([FromBody] Trip trip)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        //kontrollera om PlaceId finns
        var fromExists = await _context.Places.AnyAsync(p => p.MapServicePlaceId == trip.FromPlaceId);
        var toExists = await _context.Places.AnyAsync(p => p.MapServicePlaceId == trip.ToPlaceId);

        if (!fromExists || !toExists)
        {
            return BadRequest("Start- eller slutplatsen finns inte.");
        }

        trip.AppUserId = userId;
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrip), new { id = trip.TripId }, trip);
    }

    // DELETE: api/Trip/{id}
    //tar bort en sparad resa för inloggad användare
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == id && t.AppUserId == userId);

        if (trip == null)
        {
            return NotFound(new { message = "Resan hittades inte." });
        }

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Resan har tagits bort." });
    }
}
