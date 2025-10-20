namespace DungeonCrawler_Game_Service.Models;

public static class ApiRoutes
{
    public const string Base = "/api";
    public const string Dungeon = Base + "/dungeon";

    // Génération d'un donjon
    public const string GenerateDungeon = Dungeon;
    
    public const string GetDungeonById = Dungeon + "{id:int}/";
    

    // Entrer dans une salle
    public const string EnterRoom = Dungeon + "/enter";

    // Récupérer les salles accessibles depuis la salle actuelle
    public const string NextRooms = GetDungeonById + "next";

    // Lier deux salles
    public const string LinkRooms = Dungeon + "/link";
}
