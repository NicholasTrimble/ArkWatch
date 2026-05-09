namespace ArkWatch.Server.Models
{
    public class Alert
    {
        public int Id { get; set; }
        public string SourceId { get; set; } = string.Empty; // unique id for National Weather Service
        public string Headline { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty;
        public string DetailedInstructions { get; set; } = string.Empty;
        public DateTime SystemTimestamp { get; set; } = DateTime.UtcNow;
        public record NwsResponse(List<NwsFeature> Features); //National Weather Service API response structure
        public record NwsFeature(NwsProperties Properties); //National Weather Service Feature structure
        public record NwsProperties(string Id, string Event, string Severity, string Description); //National Weather Service Properties structure


    }
}
