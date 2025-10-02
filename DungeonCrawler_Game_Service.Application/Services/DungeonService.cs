using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;

namespace DungeonCrawler_Game_Service.Application.Services;
public class DungeonService(
    IUnitOfWork unitOfWork
) : IDungeonService
{
    private readonly Random _random = new Random();
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();

    public async Task<Dungeon> GenerateDungeonAsync()
    {
        var dungeon = new Dungeon();

        int levelsCount = _random.Next(15, 21);

        for (int levelIndex = 1; levelIndex <= levelsCount; levelIndex++)
        {
            var level = new Level { Number = levelIndex };

            int roomsInLevel;

            if (levelIndex == levelsCount)
            {
                // Dernier étage = boss (1 salle obligatoire)
                roomsInLevel = 1;
            }
            else
            {
                // Tirage pondéré : favorise 3 salle (50% chance 3, 30% 2, 10% 3)
                int roll = _random.Next(100);
                if (roll < 50)
                    roomsInLevel = 3;
                else if (roll < 80)
                    roomsInLevel = 2;
                else
                    roomsInLevel = 1;
            }

            // Création des salles pour cet étage
            for (int i = 0; i < roomsInLevel; i++)
            {
                var room = new Room
                {
                    Id = $"{levelIndex}{(char)('a' + i)}",
                    Type = levelIndex == levelsCount
                        ? RoomType.Boss
                        : GetRandomRoomType() // fonction pour tirer Monster, Treasure, Trap
                };

                level.Rooms.Add(room);
            }

            // Relie les salles au niveau suivant
            if (levelIndex < levelsCount)
            {
                var nextRoomId = $"{levelIndex + 1}a";
                foreach (var room in level.Rooms)
                {
                    room.NextRoomId = nextRoomId;
                }
            }
            else
            {
                foreach (var room in level.Rooms)
                {
                    room.NextRoomId = null;
                }
            }

            dungeon.Levels.Add(level);
        }

        // Sauvegarde en base
        await _dungeonRepository.AddAsync(dungeon);

        return dungeon;
    }

    // Tirage aléatoire entre Monster, Treasure
    private RoomType GetRandomRoomType()
    {
        var types = new[] { RoomType.Monster, RoomType.Treasure, RoomType.Trap };
        return types[_random.Next(types.Length)];
    }

    
    public List<Room> GetNextRooms(string dungeonId, string roomId)
    {
        var dungeon = _dungeonRepository.GetByIdAsync(dungeonId).Result;
        if (dungeon == null)
            throw new KeyNotFoundException("Dungeon not found");

        // Trouve la salle cliquée
        var currentRoom = dungeon.Levels.SelectMany(l => l.Rooms).FirstOrDefault(r => r.Id == roomId);
        if (currentRoom == null)
            throw new KeyNotFoundException("Room not found");

        // Retourne **toutes les salles dont le NextRoomId correspond à currentRoom.NextRoomId**
        var nextRooms = dungeon.Levels
            .SelectMany(l => l.Rooms)
            .Where(r => r.Id == currentRoom.NextRoomId || r.Id.StartsWith(currentRoom.NextRoomId?.Substring(0, 1) ?? ""))
            .ToList();

        return nextRooms;
    }

    public RoomProgress EnterRoom(string dungeonId, string nextRoomId)
    {
        var dungeon = _dungeonRepository.GetByIdAsync(dungeonId).Result;
        if (dungeon == null)
            throw new KeyNotFoundException("Dungeon not found");

        var room = dungeon.Levels.SelectMany(l => l.Rooms).FirstOrDefault(r => r.Id == nextRoomId);
        if (room == null) throw new KeyNotFoundException("Next room not found");

        var level = dungeon.Levels.FirstOrDefault(l => l.Rooms.Any(r => r.Id == nextRoomId));

        
        return new RoomProgress
        {
            RoomId = room.Id,
            Level = level?.Number ?? 0,
            Events = new List<string> { "monster", "loot" } // Placeholder pour test
        };
    }
}