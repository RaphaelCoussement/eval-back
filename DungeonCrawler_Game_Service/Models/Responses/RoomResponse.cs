namespace DungeonCrawler_Game_Service.Models.Responses;

public class RoomResponse
{
    public required string Id { get; set; }
    public required string NextRoomId { get; set; }
    public required string Type { get; set; }
}