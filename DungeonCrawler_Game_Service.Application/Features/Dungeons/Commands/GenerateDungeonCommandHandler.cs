using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;

/// <summary>
/// Command Handler pour générer un nouveau donjon.
/// </summary>
public class GenerateDungeonCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<GenerateDungeonCommand, Dungeon>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();
    private readonly Random _random = new();
    
    /// <summary>
    /// Handle génère un donjon avec un nombre aléatoire d'étages (15-20).
    /// </summary>
    /// <returns>Le nouveau donjon</returns>
    public async Task<Dungeon> Handle(GenerateDungeonCommand request, CancellationToken cancellationToken)
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
}