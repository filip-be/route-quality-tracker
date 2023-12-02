namespace RouteQualityTracker.Core.Models;

public class RouteQualityRecord
{
    public DateTimeOffset Date { get; set; }
    public RouteQualityEnum RouteQuality { get; set; }
}

