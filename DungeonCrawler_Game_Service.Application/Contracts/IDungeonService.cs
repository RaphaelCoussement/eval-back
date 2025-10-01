using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;

namespace DungeonCrawler_Game_Service.Application.Contracts;

public interface IDungeonService
{
    Task<Dungeon> GenerateDungeonAsync();
    List<Room> GetNextRooms(string dungeonId, string roomId);

    RoomProgress EnterRoom(string dungeonId, string currentRoomId, string nextRoomId);
}
