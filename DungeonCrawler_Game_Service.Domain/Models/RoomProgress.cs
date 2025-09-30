namespace DungeonCrawler_Game_Service.Domain.Models;

public class RoomProgress
{
    public string RoomId { get; set; }
    public int Level { get; set; }
    public List<string> Events { get; set; } = new();
}
