namespace DungeonCrawler_Game_Service.Models.Requests;

public class EnterRoomRequest
{
    public string DungeonId { get; set; }
    public string NextRoomId { get; set; }
}