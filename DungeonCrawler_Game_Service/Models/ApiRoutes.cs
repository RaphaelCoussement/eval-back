namespace DungeonCrawler_Game_Service.Models;

public static class ApiRoutes
{
    public const string BaseRoute = "api/";
    public const string Dungeon = BaseRoute + "dungeon/";
    public const string EnterRoom = $"{Dungeon}/enter";
    public const string NextRooms = $"{Dungeon}/next";
    public const string LinkRooms = $"{Dungeon}/link";
}