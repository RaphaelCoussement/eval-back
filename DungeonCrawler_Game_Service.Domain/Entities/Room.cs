using System.Text.Json.Serialization;

namespace DungeonCrawler_Game_Service.Domain.Entities;

public abstract class Room
{
    public string Id { get; set; } = string.Empty;
    public int Number { get; set; }
    public bool Open { get; set; } = false;
    public string NextRoomId { get; set; } = string.Empty;
    public required RoomType Type { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomType
{
    Entrance,
    CombatRoom,
    TreasureRoom,
    TrapRoom,
    BossRoom
}