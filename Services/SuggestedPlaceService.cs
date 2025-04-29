using System.Text;
using System.Text.Json;
using AvstickareApi.Models;
using PolylinerNet;

namespace AvstickareApi.Services;

public class SuggestedPlaceService(HttpClient http, IConfiguration config)
{
    private readonly HttpClient _http = http;
    private readonly string _apiKey = config["GoogleApi:ApiKey"] ?? throw new Exception("Google API-nyckel saknas.");

    //hämta föreslagna POIs längs en polyline via places API
    public async Task<List<PlaceDetails>> GetSuggestedPlacesAlongRoute(string polyline)
    {
        //avkoda polyline
        var polyliner = new Polyliner();
        var points = polyliner.Decode(polyline);

        var places = new List<PlaceDetails>();
        const int step = 10;

        //begränsar svaren för att det inte ska urarta
        const int maxTotalResults = 50;

        for (int i = 0; i < points.Count && places.Count < maxTotalResults; i += step)
        {
            var point = points[i];

            //skapar förfrågan till google places API
            var body = new
            {
                includedTypes = new[] { "tourist_attraction", "museum", "park", "restaurant", "art_gallery" },
                maxResultCount = 10,
                 //skapar en cirkel i mitten av varje punkt med en radie på 5km
                locationRestriction = new
                {
                    circle = new
                    {
                        center = new { latitude = point.Latitude, longitude = point.Longitude },
                        radius = 5000.0
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            
            //förfrågan
            var request = new HttpRequestMessage(HttpMethod.Post, "https://places.googleapis.com/v1/places:searchNearby");
            //lägger till innehåll och headers
            request.Content = content;
            request.Headers.Add("X-Goog-Api-Key", _apiKey);
            request.Headers.Add("X-Goog-FieldMask", "places.displayName,places.formattedAddress,places.location,places.id");

            //svaret
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Kunde inte anropa Google Places API. Försök igen.");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            //kolla om det finns places, annars hoppa över
            if (!doc.RootElement.TryGetProperty("places", out var array))
            {
                continue;
            }

            foreach (var result in array.EnumerateArray())
            {
                places.Add(new PlaceDetails
                {
                    Id = result.GetProperty("id").GetString(),
                    Name = result.GetProperty("displayName").GetProperty("text").GetString(),
                    Address = result.TryGetProperty("formattedAddress", out var addressElement) ? addressElement.GetString() : null,
                    Latitude = result.GetProperty("location").GetProperty("latitude").GetDouble(),
                    Longitude = result.GetProperty("location").GetProperty("longitude").GetDouble()
                });
            }
        }

        return places;
    }
}
