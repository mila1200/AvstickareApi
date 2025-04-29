namespace AvstickareApi.Models
{
    //en plats som ingår i en specifik resa.
    public class TripStop
    {
        public int TripStopId { get; set; }

        //vilken resa
        public int TripId { get; set; }
        public Trip? Trip { get; set; }

        //google place id
        public string? MapServicePlaceId {get; set;}

        //för att underlätta sortering
        public int Order { get; set; }
    }
}