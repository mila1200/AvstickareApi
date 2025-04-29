using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{
    //en sparad resa
    public class Trip
    {
        public int TripId { get; set; }
        public string? AppUserId { get; set; }
        public AppUser? User { get; set; }

        public string Name { get; set; } = $"Min resa {DateTime.UtcNow:yyyy-MM-dd}";

        //startplats, koppling till placeId
        [Required]
        public string? FromPlaceId { get; set; }

        public Place? FromPlace { get; set; }
        
        //slutplats, koppling till placeId
        [Required]
        public string? ToPlaceId { get; set; }
        
        public Place? ToPlace { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //en resa kan innehålla flera stopp
        public ICollection<TripStop>? TripStops { get; set; }
    }
}