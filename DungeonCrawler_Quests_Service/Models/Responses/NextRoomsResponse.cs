namespace DungeonCrawler_Quests_Service.Models.Responses;

public class NextRoomsResponse
{
    public required string CurrentRoomId { get; set; }
    public required List<RoomResponse> NextRooms { get; set; }
}