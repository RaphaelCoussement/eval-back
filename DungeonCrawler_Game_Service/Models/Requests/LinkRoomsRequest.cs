namespace DungeonCrawler_Game_Service.Models.Requests;

public class LinkRoomsRequest
{
    public string DungeonId { get; set; }
    public string FromRoomId { get; set; }
    public string ToRoomId { get; set; }
}