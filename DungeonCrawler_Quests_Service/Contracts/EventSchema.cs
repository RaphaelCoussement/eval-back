namespace DungeonCrawler_Quests_Service.Models;

public class EventSchema
{
    public string EventId { get; set; } = null!;
    public string EventVersion { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public object Data { get; set; } = null!;
}