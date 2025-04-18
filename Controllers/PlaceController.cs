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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Place>>> GetPlaces()
        {
            return await _context.Places.ToListAsync();
        }

        // GET: api/Place/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Place>> GetPlace(int id)
        {
            var place = await _context.Places.FindAsync(id);

            if (place == null)
            {
                return NotFound();
            }

            return place;
        }

        [HttpGet("details/{mapServicePlaceId}")]
        public async Task<ActionResult<PlaceDetails>> GetPlaceDetails(string mapServicePlaceId)
        {
            try
            {
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
