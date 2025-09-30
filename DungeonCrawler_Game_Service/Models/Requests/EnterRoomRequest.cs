namespace DungeonCrawler_Game_Service.Models.Requests;

public class EnterRoomRequest
{
    public string CurrentRoomId { get; set; }
    public string NextRoomId { get; set; }
}