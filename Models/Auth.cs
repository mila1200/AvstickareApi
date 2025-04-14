namespace AvstickareApi.Models
{
    //klass för att underlätta och skydda lösenordsbyte, skickas ej till databas
    public class ChangePassword
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}

