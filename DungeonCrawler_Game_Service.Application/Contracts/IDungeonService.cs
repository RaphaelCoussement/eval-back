using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;

namespace DungeonCrawler_Game_Service.Application.Contracts;

public interface IDungeonService
{
    Dungeon GenerateDungeon(int minRooms, int maxRooms);
    List<Room> GetNextRooms(string dungeonId, string roomId);

    RoomProgress EnterRoom(string dungeonId, string currentRoomId, string nextRoomId);
}
