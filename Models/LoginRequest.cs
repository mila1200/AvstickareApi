using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{

    //för att hantera inloggning
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}