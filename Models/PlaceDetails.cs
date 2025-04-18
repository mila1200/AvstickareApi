namespace AvstickareApi.Models
{
    public class PlaceDetails
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public double? Rating { get; set; }
        public List<string>? OpeningHours { get; set; }
        public string? Photo { get; set; }
    }
}