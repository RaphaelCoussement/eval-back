namespace DungeonCrawler_Quests_Service.Models.Responses;

public class DungeonResponse
{
    public required string Id { get; set; }
    public List<LevelResponse> Levels { get; set; } = new();
}

public class LevelResponse
{
    public int Number { get; set; }
    public List<RoomResponse> Rooms { get; set; } = new();
}