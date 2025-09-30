namespace DungeonCrawler_Game_Service.Models.Requests;

public class GenerateDungeonRequest
{
    public int MinRooms { get; set; } = 15;
    public int MaxRooms { get; set; } = 20;
}