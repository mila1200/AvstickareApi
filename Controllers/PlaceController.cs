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

//visa plats, platsdetaljer eller sökning

namespace AvstickareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly AvstickareContext _context;
        private readonly GoogleMapsService _mapsService;

        public PlaceController(AvstickareContext context, GoogleMapsService mapsService)
        {
            _context = context;
            _mapsService = mapsService;
        }

        // GET: api/Place
        //hämta en lista med alla platser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Place>>> GetPlaces()
        {
            //hämta endast den info frontend är intresserad av.
            var places = await _context.Places
            .Include(p => p.Category)
        .Select(p => new
        {
            p.PlaceId,
            p.Name,
            p.Lat,
            p.Lng,
            p.MapServicePlaceId,
            Category = p.Category != null ? p.Category.Name : null
        })
        .ToListAsync();

            //kollar om det finns några platser
            if (places == null || !places.Any())
            {
                return NotFound("Det finns inga platser att hämta.");
            }

            return Ok(places);
        }

        // GET: api/Place/5
        //hämtar detlajerad info om plats baserat på id
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPlace(int id)
        {
            //hämta plats med id och returnera relevant info
            var place = await _context.Places
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.PlaceId == id);

            if (place == null)
            {
                return NotFound(new { message = $"Platsen med id {id} hittades inte." });
            }

            return Ok(new
            {
                place.PlaceId,
                place.MapServicePlaceId,
                place.Name,
                place.Lat,
                place.Lng,
                Category = place.Category?.Name
            });
        }

        //hämta info om en plats från google baserat på deras id
        [HttpGet("details/{mapServicePlaceId}")]
        public async Task<ActionResult<PlaceDetails>> GetPlaceDetails(string mapServicePlaceId)
        {
            try
            {   //anropa service för att hämta data
                var details = await _mapsService.GetPlaceDetails(mapServicePlaceId);
                return Ok(details);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
