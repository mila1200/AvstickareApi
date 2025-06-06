using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{
    //favoriter, koppling mellan användare och platser som är sparade
    public class FavoritePlace
    {
        public int FavoritePlaceId { get; set; }

        //behöver veta vilken användare som sparat platsen
        [Required]
        public string? AppUserId { get; set; }
        public AppUser? User { get; set; }

        //behöver veta vilken plats det gäller
        public string? MapServicePlaceId { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}