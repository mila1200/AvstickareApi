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

    
    //lägger till en plats i favoriter för inloggad användare.
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] FavoritePlace favoritePlace)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(favoritePlace.MapServicePlaceId))
            return BadRequest("Ogiltigt plats-ID.");

        await _placeService.EnsurePlaceExists(favoritePlace.MapServicePlaceId);

        favoritePlace.AppUserId = userId;
        _context.FavoritePlaces.Add(favoritePlace);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFavorites), new { id = favoritePlace.FavoritePlaceId }, favoritePlace);
    }

    
    //tar bort en favoritplats för inloggad användare.
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var favorite = await _context.FavoritePlaces
            .FirstOrDefaultAsync(f => f.FavoritePlaceId == id && f.AppUserId == userId);

        if (favorite == null)
        {
            return NotFound(new { message = "Favoritplatsen hittades inte." });
        }
            
        _context.FavoritePlaces.Remove(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Favoritplatsen har tagits bort." });
    }
}