using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{
    //plats för att visa på kartan
    public class Place
    {
        public int PlaceId { get; set; }

        //required om platsen ska sparas i databasen trots att användaren inte aktivt fyller i något
        [Required]
        public string? Name { get; set; }

        //latitud och longitud
        public double Lat { get; set; }
        public double Lng { get; set; }

        //för att kunna filtrera med kategorier
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        //för att kunna hänvisa till platsen via karttjänsten
        public string? MapServicePlaceId { get; set; }

        //relation till favoriter och sparade resestopp
        public ICollection<FavoritePlace>? FavoritePlaces { get; set; }
        public ICollection<TripStop>? TripStops { get; set; }
    }
}