namespace AvstickareApi.Models
{
    //en plats som ingår i en specifik resa.
    public class TripStop
    {
        public int TripStopId { get; set; }

        //vilken resa
        public int TripId { get; set; }
        public Trip? Trip { get; set; }

        //vilka stopp
        public int PlaceId { get; set; }
        public Place? Place { get; set; }

        //för att underlätta sortering av stopp
        public int Order { get; set; }
    }
}