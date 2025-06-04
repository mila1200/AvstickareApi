using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{
    //modell för användare
    public class AppUser
    {
        //props
        public string AppUserId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)]
        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //relation till favoriteplace och trip
        public ICollection<FavoritePlace>? FavoritePlaces { get; set; }
        public ICollection<Trip>? Trips { get; set; }
    }
}