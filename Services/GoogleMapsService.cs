using System.Globalization;
using System.Text;
using System.Text.Json;
using AvstickareApi.Data;
using AvstickareApi.Models;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<Place>> CreatePlace(string polyline)
        {
            //avkoda polyline
            var polyliner = new Polyliner();
            var points = polyliner.Decode(polyline);

            var places = new List<Place>();
            const int step = 10;

            for (int i = 0; i < points.Count; i += step)
            {
                var point = points[i];

                //skapar förfrågan till Google places api (new)
                var requestBody = new
                {
                    includedTypes = new[] { "tourist_attraction", "museum", "park", "restaurant", "art_gallery" },
                    maxResultCount = 10,
                    //skapar en cirkel i mitten av varje punkt med en radie på 5km
                    locationRestriction = new
                    {
                        circle = new
                        {
                            center = new
                            {
                                latitude = point.Latitude,
                                longitude = point.Longitude
                            },
                            radius = 5000.0
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                //förfrågan
                var request = new HttpRequestMessage(HttpMethod.Post, "https://places.googleapis.com/v1/places:searchNearby");
                //lägger till innehåll och headers
                request.Content = content;
                request.Headers.Add("X-Goog-Api-Key", _apiKey);
                request.Headers.Add("X-Goog-FieldMask", "places.displayName,places.formattedAddress,places.location,places.id");

                //svaret
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Kunde inte anropa Google Places API. Försök igen.");
                }

                using var doc = JsonDocument.Parse(json);

                //kolla om det finns places, annars hoppa över
                if (!doc.RootElement.TryGetProperty("places", out var placesArray)) continue;

                //loopa igenom listan
                foreach (var result in placesArray.EnumerateArray())
                {
                    var name = result.GetProperty("displayName").GetProperty("text").GetString();
                    var lat = result.GetProperty("location").GetProperty("latitude").GetDouble();
                    var lng = result.GetProperty("location").GetProperty("longitude").GetDouble();
                    var placeId = result.GetProperty("id").GetString();

                    //skapa ny plats
                    places.Add(new Place
                    {
                        Name = name ?? "Platsnamn saknas",
                        Lat = lat,
                        Lng = lng,
                        MapServicePlaceId = placeId,
                        CategoryId = 1
                    });
                }
            }
            return places;
        }

        //hämta platsdetaljer
        public async Task<PlaceDetails> GetPlaceDetails(string mapServicePlaceId)
        {
            //anropar api med platsens id från google
            var url = $"https://places.googleapis.com/v1/places/{mapServicePlaceId}";

            //skickar förfrågan med nyckel och vilka fält som ska hämtas
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Goog-Api-Key", _apiKey);
            request.Headers.Add("X-Goog-FieldMask", "id,displayName,formattedAddress,internationalPhoneNumber,websiteUri,rating,regularOpeningHours.weekdayDescriptions,photos");

            //svaret   
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Kunde inte hämta platsdetaljer från Google Places API.");
            }

            //läser ut som json
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            //plockar ut info, finns de inte så är de null
            var id = root.GetProperty("id").GetString();
            var name = root.GetProperty("displayName").GetProperty("text").GetString();
            var address = root.TryGetProperty("formattedAddress", out var addressElement) ? addressElement.GetString() : null;
            var phone = root.TryGetProperty("internationalPhoneNumber", out var phoneElement) ? phoneElement.GetString() : null;
            var website = root.TryGetProperty("websiteUri", out var websiteElement) ? websiteElement.GetString() : null;
            var rating = root.TryGetProperty("rating", out var ratingElement) ? ratingElement.GetDouble() : (double?)null;

            //äppettider, kollar omd det finns info, letar efter weekdayDescriptions och samlar i lista
            List<string>? openingHours = null;
            if (root.TryGetProperty("regularOpeningHours", out var hoursElement) && hoursElement.TryGetProperty("weekdayDescriptions", out var weekdayArray))
            {
                openingHours = weekdayArray.EnumerateArray()
                                           .Select(d => d.GetString())
                                           .Where(s => s != null)
                                           .ToList()!;
            }

            //foto, kollar om det finns och skapar i så fall bild-URL för nytt anrop
            string? photoName = null;
            if (root.TryGetProperty("photos", out var photosArray) && photosArray.GetArrayLength() > 0)
            {
                photoName = photosArray[0].GetProperty("name").GetString();
            }

            //rerunrerar info som objekt
            return new PlaceDetails
            {
                Id = id,
                Name = name,
                Address = address,
                Phone = phone,
                Website = website,
                Rating = rating,
                OpeningHours = openingHours,
                Photo = photoName
            };
        }
    }
}