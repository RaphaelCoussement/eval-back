using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;

namespace DungeonCrawler_Game_Service.Application.Services;
public class DungeonService : IDungeonService
{
    private readonly Random _random = new Random();
    private readonly IRepository<Dungeon> _dungeonRepository;

    public DungeonService(IUnitOfWork unitOfWork)
    {
        _dungeonRepository = unitOfWork.GetRepository<Dungeon>();
    }

    public Dungeon GenerateDungeon(int minRooms, int maxRooms)
    {
        var dungeon = new Dungeon();

        // Nombre total de salles à générer
        int totalRooms = _random.Next(minRooms, maxRooms + 1);

        // Minimum 15 étages
        int levelsCount = Math.Max(15, totalRooms / 3); // Chaque étage peut avoir max 3 salles

        int roomsCreated = 0;

        for (int levelIndex = 1; levelIndex <= levelsCount; levelIndex++)
        {
            var level = new Level { Number = levelIndex };

            // Dernier étage = boss (1 salle)
            int roomsInLevel;
            if (levelIndex == levelsCount)
            {
                roomsInLevel = 1;
            }
            else
            {
                // On prend 1 à 3 salles par étage, mais sans dépasser le total de salles
                roomsInLevel = Math.Min(_random.Next(1, 4), totalRooms - roomsCreated - (levelsCount - levelIndex));
            }

            for (int i = 0; i < roomsInLevel; i++)
            {
                var room = new Room
                {
                    Id = $"{levelIndex}{(char)('a' + i)}"
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
                // Dernière salle = boss
                foreach (var room in level.Rooms)
                {
                    room.NextRoomId = null;
                }
            }

            dungeon.Levels.Add(level);
            roomsCreated += roomsInLevel;
        }

        // Sauvegarde en base
        _dungeonRepository.AddAsync(dungeon).Wait();

        return dungeon;
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

    public RoomProgress EnterRoom(string dungeonId, string currentRoomId, string nextRoomId)
    {
        var dungeon = _dungeonRepository.GetByIdAsync(dungeonId).Result;
        if (dungeon == null)
            throw new KeyNotFoundException("Dungeon not found");

        var room = dungeon.Levels.SelectMany(l => l.Rooms).FirstOrDefault(r => r.Id == nextRoomId);
        if (room == null) throw new KeyNotFoundException("Next room not found");

        var level = dungeon.Levels.FirstOrDefault(l => l.Rooms.Any(r => r.Id == nextRoomId));

        // 👉 Ici, tu pourrais aussi sauvegarder une "progression du joueur" dans une autre collection Mongo
        return new RoomProgress
        {
            RoomId = room.Id,
            Level = level?.Number ?? 0,
            Events = new List<string> { "monster", "loot" } // Placeholder pour test
        };
    }
}