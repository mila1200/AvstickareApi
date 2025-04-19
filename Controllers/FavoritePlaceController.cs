using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvstickareApi.Data;
using AvstickareApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

//lägg till, ta bort/lista favoriter

namespace AvstickareApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FavoritePlaceController : ControllerBase
    {
        private readonly AvstickareContext _context;

        public FavoritePlaceController(AvstickareContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetFavoritePlaces()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //hämta avoriter där platsen inte är null
            var favorites = await _context.FavoritePlaces
                .Where(f => f.AppUserId == userId)
                .Include(f => f.Place)
                .ToListAsync();

            //kopplar resultat till favoriter och löser null-fel
            var result = favorites
                .Where(f => f.Place != null)
                .Select(f => new
                {
                    f.FavoritePlaceId,
                    f.Place!.PlaceId,
                    f.Place.MapServicePlaceId,
                    f.SavedAt
                });

            return Ok(result);
        }

        //hämta favoriter baserat på id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<object>> GetFavoritePlace(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//hämta favorit där id och användare stämmer
            var favorite = await _context.FavoritePlaces
                .Include(f => f.Place)
                .FirstOrDefaultAsync(f => f.FavoritePlaceId == id && f.AppUserId == userId);

            if (favorite == null)
            {
                return NotFound("Favoriten hittades inte.");
            }

            if (favorite.Place == null)
            {
                return BadRequest("Favoriten är kopplad till en plats som inte längre finns.");
            }

            return Ok(new
            {
                favorite.FavoritePlaceId,
                favorite.Place.PlaceId,
                favorite.Place.MapServicePlaceId,
                favorite.SavedAt
            });
        }

        //lägg till en plats som favorit
        // POST: api/FavoritePlace/{placeId}
        [HttpPost("{placeId}")]
        public async Task<ActionResult<FavoritePlace>> AddFavoritePlace(int placeId)
        {
            //användare
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //kontrollera om platsen finns
            var placeExists = await _context.Places.AnyAsync(place => place.PlaceId == placeId);
            if (!placeExists)
            {
                return NotFound("Platsen hittades inte.");
            }

            // kontrollera dubbletter
            var favoriteExists = await _context.FavoritePlaces
                .AnyAsync(favoriteplace => favoriteplace.AppUserId == userId && favoriteplace.PlaceId == placeId);

            if (favoriteExists)
            {
                return BadRequest("Platsen är redan sparad som favorit.");
            }

            //skapa och spara favorit med användarid och platsid
            var favoritePlace = new FavoritePlace
            {
                AppUserId = userId,
                PlaceId = placeId,
            };

            _context.FavoritePlaces.Add(favoritePlace);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFavoritePlace), new { id = favoritePlace.FavoritePlaceId }, favoritePlace);
        }

        //ta bort favorit
        // DELETE: api/FavoritePlace/{placeId}
        [HttpDelete("{placeId}")]
        public async Task<IActionResult> RemoveFavoritePlace(int placeId)
        {
            //användare
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoritePlace = await _context.FavoritePlaces
                .FirstOrDefaultAsync(favoriteplace => favoriteplace.AppUserId == userId && favoriteplace.PlaceId == placeId);

            //finns den?
            if (favoritePlace == null)
            {
                return NotFound("Favoriten hittades inte.");
            }

            //ta bort
            _context.FavoritePlaces.Remove(favoritePlace);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Platsen har tagits bort från favoriter." });
        }
    }
}
