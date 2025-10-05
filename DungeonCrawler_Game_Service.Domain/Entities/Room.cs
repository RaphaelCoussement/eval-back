namespace DungeonCrawler_Game_Service.Domain.Entities;

public abstract class Room
{
    public int Number { get; set; }
    public bool Open { get; set; }
}