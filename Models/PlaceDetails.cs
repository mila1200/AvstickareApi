using System.ComponentModel.DataAnnotations.Schema;

namespace AvstickareApi.Models
{
    //kan inte lagra uppgifter om ett resmål mer än tillfälligt enligt Google TOS.
    [NotMapped]
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
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}