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
        public string? PasswordHash { get; set; }

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //relation till favoriteplace och trip
        public ICollection<FavoritePlace>? FavoritePlaces { get; set; }
        public ICollection<Trip>? Trips { get; set; }
    }
}