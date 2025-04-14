namespace AvstickareApi.Models
{

    //för att hantera inloggning
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}