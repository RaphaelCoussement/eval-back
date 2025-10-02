using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;

namespace DungeonCrawler_Game_Service.Application.Contracts;

public interface IDungeonService
{
    /// <summary>
    /// Génération d'un donjon
    /// </summary>
    /// <returns>Un donjon</returns>
    Task<Dungeon> GenerateDungeonAsync();
    
    /// <summary>
    /// Récupération des prochaines salles accessibles depuis une salle donnée
    /// </summary>
    /// <param name="dungeonId">Donjon concerné</param>
    /// <param name="roomId">Salle actuelle</param>
    /// <returns>Une liste de salle</returns>
    List<Room> GetNextRooms(string dungeonId, string roomId);

    /// <summary>
    /// Cible la salle suivante
    /// </summary>
    /// <param name="dungeonId">Le donjon dans lequel est la salle</param>
    /// <param name="nextRoomId">Salle suivante</param>
    /// <returns></returns>
    RoomProgress EnterRoom(string dungeonId, string nextRoomId);
}
