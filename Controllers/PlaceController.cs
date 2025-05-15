using AvstickareApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvstickareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaceController(PlaceService placeService) : ControllerBase
{
    private readonly PlaceService _placeService = placeService;

    //hämtar detaljerad platsinformation från Google Places API baserat på ett Google PlaceId.
    [HttpGet("{placeId}")]
    public async Task<IActionResult> GetPlaceDetails(string placeId)
    {
        try
        {
            var placeDetails = await _placeService.GetPlaceDetails(placeId);
            return Ok(placeDetails);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}