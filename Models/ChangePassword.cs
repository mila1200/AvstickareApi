using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvstickareApi.Models
{
    //klass för att underlätta och skydda lösenordsbyte, skickas ej till databas
    [NotMapped]
    public class ChangePassword
    {
        [Required]
        public string? OldPassword { get; set; }
        [Required]
        public string? NewPassword { get; set; }
    }
}

