using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{
    //en sparad resa
    public class Trip
    {
        public int TripId { get; set; }
        [Required]
        public string? AppUserId { get; set; }
        public AppUser? User { get; set; }

        public string Name { get; set; } = $"Min resa {DateTime.UtcNow:yyyy-MM-dd}";
        [Required]
        public string? TripFrom { get; set; }
        [Required]
        public string? TripTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //en resa kan innehålla flera stopp
        public ICollection<TripStop>? TripStops { get; set; }
    }
}