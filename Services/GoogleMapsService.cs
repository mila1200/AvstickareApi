using System.Text.Json;
using AvstickareApi.Data;
using AvstickareApi.Models;
using PolylinerNet;

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

            //svaret
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            //kontrollera statusfält
            if (!root.TryGetProperty("status", out var statusElement) || statusElement.GetString() != "OK")
            {
                throw new Exception("Kunde inte hämta koordinater.");
            }

            // Hämta första resultatet i "results"-arrayen
            var firstResult = root.GetProperty("results").EnumerateArray().FirstOrDefault();

            // Inga resultat
            if (firstResult.ValueKind == JsonValueKind.Undefined)
            {
                throw new Exception("Ingen plats hittades.");
            }

            // Hämta lat/lng från geometry - location
            var locationElement = firstResult.GetProperty("geometry").GetProperty("location");

            var lat = locationElement.GetProperty("lat").GetDouble();
            var lng = locationElement.GetProperty("lng").GetDouble();

            return (lat, lng);
        }

        //skapar en resa
        public async Task<(string? polyline, string? distance, string? duration)> CreateTrip(Trip trip)
        {
            //måste vara ifylld
            if (string.IsNullOrWhiteSpace(trip.TripFrom) || string.IsNullOrWhiteSpace(trip.TripTo))
            {
                throw new ArgumentException("Resan måste ha en start- och en slutdestination");
            }

            var fromCoordinates = await GetLatLng(trip.TripFrom);
            var toCoordinates = await GetLatLng(trip.TripTo);

            //spara koordinater
            var fromLat = trip.FromLat = fromCoordinates.lat;
            var fromLng = trip.FromLng = fromCoordinates.lng;
            var toLat = trip.ToLat = toCoordinates.lat;
            var toLng = trip.ToLng = toCoordinates.lng;

            if (fromLat == null || fromLng == null || toLat == null || toLng == null)
            {
                throw new Exception("Koordinater saknas.");
            }

            //omvandlar till stäng med . istället för , för att det ska fungera med google.
            string fromLatStr = fromLat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string fromLngStr = fromLng.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string toLatStr = toLat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string toLngStr = toLng.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

            Console.WriteLine($"lat och long; {fromLatStr}, {fromLngStr}, {toLatStr}, {toLngStr}");

            //hämta rutt från api
            var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={fromLatStr},{fromLngStr}&destination={toLatStr},{toLngStr}&mode=driving&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Kunde inte anropa Google API. Försök igen.");
            }

            //svaret
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            // Hämta första resultatet i "results"-arrayen
            var route = doc.RootElement.GetProperty("routes").EnumerateArray().FirstOrDefault();

            if (route.ValueKind == JsonValueKind.Undefined)
            {
                throw new Exception("Kunde inte skapa resa.");
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

        //Avkoda polylines för att lägga till ruttstopp
        public async Task<List<Place>> CreatePlace(string polyline, AvstickareContext avstickareContext)
        {
            //avkoda polyline
            var polyliner = new Polyliner();
            var points = polyliner.Decode(polyline);

            var places = new List<Place>();

            //loopa igenom
            foreach (var point in points)
            {
                //hämta namnet på resmålet genom reverse geocoding
                var name = await GetPlaceName(point.Latitude, point.Longitude);

                //skapa nytt place OBS!!!! HANTERA CATEGORY SENARE!!
                var place = new Place { Name = name ?? "Resmål", Lat = point.Latitude, Lng = point.Longitude, CategoryId = 1 };

                //lägger till platsen
                avstickareContext.Places.Add(place);
                places.Add(place);
            };

            //lägg till i databasen för att slippa anropa API så mycket
            await avstickareContext.SaveChangesAsync();
            return places;
        }

        //tar in lat och lng för att returnera ett platsnamn som sträng
        public async Task<string?> GetPlaceName(double lat, double lng)
        {
            //omvandla till strängar med . istället för ,
            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lngStr = lng.ToString(System.Globalization.CultureInfo.InvariantCulture);

            //anropa geocode med lat och long
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latStr},{lngStr}&key={_apiKey}";

            //svaret
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Kunde inte anropa Geocode API. Försök igen.");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            //kontrollera status
            if (!root.TryGetProperty("status", out var statusElement) || statusElement.GetString() != "OK")
            {
                throw new Exception("Geokodning misslyckades.");
            }

            //resultat
            var firstResult = root.GetProperty("results").EnumerateArray().FirstOrDefault();
            if(firstResult.ValueKind == JsonValueKind.Undefined)
            {
                throw new Exception("Inga resultat hittades.");
            }

            return firstResult.GetProperty("formatted_address").GetString();
        }


    }


}