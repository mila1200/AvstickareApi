
using System.Text.Json;

namespace AvstickareApi.Services
{
    public class GoogleMapsService(HttpClient httpClient, IConfiguration config)
    {
        private readonly HttpClient _httpClient = httpClient;
        //hämta nyckeln
        private readonly string _apiKey = config["GoogleApi:ApiKey"] ?? throw new ArgumentNullException("GoogleApi:ApiKey");

        //omvandla ort till lat/lng
        public async Task<(double? lat, double? lng)> GetLatLng(string location)
        {
            //anropa geocode api
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(location)}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Kunde inte anropa Geocode API. Försök igen.");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            //kontrollera statusfält
            if (!root.TryGetProperty("status", out var statusElement))
            {
                return (null, null);
            }

            //felstatus på förfrågan
            var status = statusElement.GetString();
            if (status != "OK")
            {
                return (null, null);
            }

            // Hämta första resultatet i "results"-arrayen
            var firstResult = root
                .GetProperty("results")
                .EnumerateArray()
                .FirstOrDefault();

            // Inga resultat
            if (firstResult.ValueKind == JsonValueKind.Undefined)
            {
                return (null, null);
            }

            // Hämta lat/lng från geometry - location
            var locationElement = firstResult
                .GetProperty("geometry")
                .GetProperty("location");

            var lat = locationElement.GetProperty("lat").GetDouble();
            var lng = locationElement.GetProperty("lng").GetDouble();

            return (lat, lng);
        }
    }
}