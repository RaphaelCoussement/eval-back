namespace DungeonCrawler_Game_Service.Domain.Entities;

public class Level
{
    public int Number { get; set; }
    public List<Room> Rooms { get; set; } = new();
}