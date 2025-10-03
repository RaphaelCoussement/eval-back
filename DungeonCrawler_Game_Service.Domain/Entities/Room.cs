namespace DungeonCrawler_Game_Service.Domain.Entities;

public class Room
{
    public string Id { get; set; }
    public string NextRoomId { get; set; }
    public RoomType Type { get; set; }
}

public enum RoomType
{
    Monster,
    Treasure,
    Trap,
    Boss,
    Entrance,
}