namespace DungeonCrawler_Game_Service.Models.Responses;

public class NextRoomsResponse
{
    public string CurrentRoomId { get; set; }
    public List<RoomResponse> NextRooms { get; set; }
}