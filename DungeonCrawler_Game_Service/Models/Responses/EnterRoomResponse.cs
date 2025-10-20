namespace DungeonCrawler_Game_Service.Models.Responses;

public class EnterRoomResponse
{
    public required string EnteredRoomId { get; set; }
    public int Level { get; set; }

    // Pour le futur : événements, ennemis, loot, etc.
    public List<string> Events { get; set; } = new List<string>();
}