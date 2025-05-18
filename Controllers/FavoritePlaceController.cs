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
public class FavoritePlaceController(AvstickareContext context, PlaceService placeService) : ControllerBase
{
    private readonly AvstickareContext _context = context;
    private readonly PlaceService _placeService = placeService;


    //hämtar alla favoriter för den inloggade användaren.
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var favorites = await _context.FavoritePlaces
            .Where(f => f.AppUserId == userId)
            .Select(f => new
            {
                f.FavoritePlaceId,
                f.MapServicePlaceId
            })
            .ToListAsync();

        return Ok(favorites);
    }

    //kolla om en plats finns sparad som favorit i databasen
    // GET: api/FavoritePlace/exists/{mapServicePlaceId}
    [HttpGet("exists/{mapServicePlaceId}")]
    [Authorize]
    public async Task<IActionResult> IsFavoritePlace(string mapServicePlaceId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(mapServicePlaceId))
        {
            return BadRequest("Plats-ID saknas.");
        }
            
        var exists = await _context.FavoritePlaces
            .AnyAsync(f => f.AppUserId == userId && f.MapServicePlaceId == mapServicePlaceId);

        return Ok(new { isFavorite = exists });
    }

    //lägger till en plats i favoriter för inloggad användare.
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] FavoritePlaceRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.MapServicePlaceId))
        {
            return BadRequest("Ogiltigt plats-ID.");
        }
            
        await _placeService.EnsurePlaceExists(request.MapServicePlaceId);

        var favorite = new FavoritePlace
        {
            AppUserId = userId,
            MapServicePlaceId = request.MapServicePlaceId
        };

        _context.FavoritePlaces.Add(favorite);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFavorites), new { id = favorite.FavoritePlaceId }, favorite);
    }

    //tar bort en favoritplats för inloggad användare.
    [Authorize]
    [HttpDelete("remove/{mapServicePlaceId}")]
    public async Task<IActionResult> DeleteFavorite(string mapServicePlaceId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var favorite = await _context.FavoritePlaces
            .FirstOrDefaultAsync(f => f.AppUserId == userId && f.MapServicePlaceId == mapServicePlaceId);

        if (favorite == null)
        {
            return NotFound(new { message = "Favoritplatsen hittades inte." });
        }

        _context.FavoritePlaces.Remove(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Favoritplatsen har tagits bort." });
    }
}