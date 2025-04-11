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
    public class FavoritePlaceController : ControllerBase
    {
        private readonly AvstickareContext _context;

        public FavoritePlaceController(AvstickareContext context)
        {
            _context = context;
        }

        // GET: api/FavoritePlace
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoritePlace>>> GetFavoritePlaces()
        {
            return await _context.FavoritePlaces.ToListAsync();
        }

        // GET: api/FavoritePlace/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FavoritePlace>> GetFavoritePlace(int id)
        {
            var favoritePlace = await _context.FavoritePlaces.FindAsync(id);

            if (favoritePlace == null)
            {
                return NotFound();
            }

            return favoritePlace;
        }

        // PUT: api/FavoritePlace/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFavoritePlace(int id, FavoritePlace favoritePlace)
        {
            if (id != favoritePlace.FavoritePlaceId)
            {
                return BadRequest();
            }

            _context.Entry(favoritePlace).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FavoritePlaceExists(id))
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

        // POST: api/FavoritePlace
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<FavoritePlace>> PostFavoritePlace(FavoritePlace favoritePlace)
        {
            _context.FavoritePlaces.Add(favoritePlace);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFavoritePlace", new { id = favoritePlace.FavoritePlaceId }, favoritePlace);
        }

        // DELETE: api/FavoritePlace/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavoritePlace(int id)
        {
            var favoritePlace = await _context.FavoritePlaces.FindAsync(id);
            if (favoritePlace == null)
            {
                return NotFound();
            }

            _context.FavoritePlaces.Remove(favoritePlace);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FavoritePlaceExists(int id)
        {
            return _context.FavoritePlaces.Any(e => e.FavoritePlaceId == id);
        }
    }
}
