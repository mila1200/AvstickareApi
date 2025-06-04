using System.Text.Json;
using AvstickareApi.Data;
using AvstickareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AvstickareApi.Services;

public class PlaceService(HttpClient http, IConfiguration config, AvstickareContext context)
{
    private readonly HttpClient _http = http;

    //databasanslutning
    private readonly AvstickareContext _context = context;
    //hämta nyckeln
    private readonly string _apiKey = config["GoogleApi:ApiKey"] ?? throw new Exception("Google API-nyckel saknas.");


    //hämta google PlaceId från ett ortnamn (via geocoding API)
    public async Task<string> GetPlaceIdFromLocation(string location)
    {
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(location)}&key={_apiKey}";

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Kunde inte hämta plats-ID från Google.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var result = doc.RootElement.GetProperty("results").EnumerateArray().FirstOrDefault();

        if (result.ValueKind == JsonValueKind.Undefined)
        {
            throw new Exception("Ingen plats hittades. Kontrollera stavningen och försök igen.");
        }

        return result.GetProperty("place_id").GetString() ?? throw new Exception("Platsens ID saknas.");
    }

    //hämta koordinater (lat/lng) från en google PlaceId (via geocoding API)
    public async Task<(double lat, double lng)> GetCoordinates(string placeId)
    {
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?place_id={placeId}&key={_apiKey}";

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Kunde inte hämta koordinater från Google.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var result = doc.RootElement.GetProperty("results").EnumerateArray().FirstOrDefault();

        var location = result.GetProperty("geometry").GetProperty("location");

        return (location.GetProperty("lat").GetDouble(), location.GetProperty("lng").GetDouble());
    }

    //kontrollera att en plats finns i databasen, annars spara den (endast PlaceId)
    public async Task EnsurePlaceExists(string placeId)
    {
        var exists = await _context.Places.AnyAsync(p => p.MapServicePlaceId == placeId);
        if (exists)
        {
            return;
        }

        _context.Places.Add(new Place { MapServicePlaceId = placeId });
        await _context.SaveChangesAsync();
    }

    //hämta platsdetaljer från Google Places API, på svenska
    public async Task<PlaceDetails> GetPlaceDetails(string placeId)
    {
        var url = $"https://places.googleapis.com/v1/places/{placeId}?languageCode=sv";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Goog-Api-Key", _apiKey);
        request.Headers.Add("X-Goog-FieldMask", "id,displayName,formattedAddress,internationalPhoneNumber,websiteUri,rating,regularOpeningHours.weekdayDescriptions,photos,location");

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Kunde inte hämta platsdetaljer.");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        //plockar ut info, finns de ej så de null
        var name = root.GetProperty("displayName").GetProperty("text").GetString();
        var address = root.TryGetProperty("formattedAddress", out var addressElement) ? addressElement.GetString() : null;
        var phone = root.TryGetProperty("internationalPhoneNumber", out var phoneElement) ? phoneElement.GetString() : null;
        var website = root.TryGetProperty("websiteUri", out var webElement) ? webElement.GetString() : null;
        var rating = root.TryGetProperty("rating", out var rateElement) ? rateElement.GetDouble() : (double?)null;

        //äppettider, kollar omd det finns info, letar efter weekdayDescriptions och samlar i lista
        List<string>? openingHours = null;
        if (root.TryGetProperty("regularOpeningHours", out var hoursElement) && hoursElement.TryGetProperty("weekdayDescriptions", out var weekdayArray))
        {
            openingHours = weekdayArray.EnumerateArray()
                                       .Select(d => d.GetString())
                                       .Where(s => s != null)
                                       .ToList()!;
        }

        //foto URL
        string? photoUrl = null;
        if (root.TryGetProperty("photos", out var photosArr) && photosArr.GetArrayLength() > 0)
        {
            var photoName = photosArr[0].GetProperty("name").GetString();
            if (!string.IsNullOrEmpty(photoName))
                photoUrl = $"https://places.googleapis.com/v1/{photoName}/media?key={_apiKey}&maxWidthPx=800";
        }

        double? latitude = null;
        double? longitude = null;
        //plockar ut koordinaterna
        if (root.TryGetProperty("location", out var locationElement))
        {
            latitude = locationElement.TryGetProperty("latitude", out var latElement) ? latElement.GetDouble() : (double?)null;
            longitude = locationElement.TryGetProperty("longitude", out var lngElement) ? lngElement.GetDouble() : (double?)null;
        }

        return new PlaceDetails
        {
            Id = placeId,
            Name = name,
            Address = address,
            Phone = phone,
            Website = website,
            Rating = rating,
            OpeningHours = openingHours,
            Photo = photoUrl,
            Latitude = latitude,
            Longitude = longitude
        };
    }
}


