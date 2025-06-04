using System.Globalization;
using System.Text.Json;

namespace AvstickareApi.Services;

public class RouteService(HttpClient http, IConfiguration config)
{
    private readonly HttpClient _http = http;
    private readonly string _apiKey = config["GoogleApi:ApiKey"] ?? throw new Exception("Google API-nyckel saknas.");

    //skapar en rutt mellan två koordinater via directions API
    public async Task<(string? polyline, string? distance, string? duration)> CreateTripRoute((double lat, double lng) from, (double lat, double lng) to)
    {
        var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={from.lat.ToString(CultureInfo.InvariantCulture)},{from.lng.ToString(CultureInfo.InvariantCulture)}&destination={to.lat.ToString(CultureInfo.InvariantCulture)},{to.lng.ToString(CultureInfo.InvariantCulture)}&mode=driving&key={_apiKey}";

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Kunde inte hämta rutt.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var route = doc.RootElement.GetProperty("routes").EnumerateArray().FirstOrDefault();
        if (route.ValueKind == JsonValueKind.Undefined)
        {
            throw new Exception("Google returnerade ingen giltig rutt.");
        }

        //hämtar delsträcka, allting i detta fallet
        var leg = route.GetProperty("legs")[0];
        //hämtar rutten som används för att rita sträckan på kartan
        var polyline = route.GetProperty("overview_polyline").GetProperty("points").GetString();
        //hämta stäckans längd som text
        var distance = leg.GetProperty("distance").GetProperty("text").GetString();
        //hämta sträckans varaktighet som text
        var duration = leg.GetProperty("duration").GetProperty("text").GetString();

        return (polyline, distance, duration);
    }
}