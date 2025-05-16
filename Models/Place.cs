using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{
    //plats för att visa på kartan
    public class Place
    {
        public int PlaceId { get; set; }

        //för att kunna hänvisa till platsen via karttjänsten
        [Required]
        public string? MapServicePlaceId { get; set; }
    }
}