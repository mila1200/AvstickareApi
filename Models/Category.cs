using System.ComponentModel.DataAnnotations;

namespace AvstickareApi.Models
{
    //typ av plats
    public class Category
    {
        public int CategoryId { get; set; }
        
        [Required]
        public string? Name { get; set; }

        //koppling till platser för att kunna kategorisera
        public ICollection<Place>? Places { get; set; }
    }
}