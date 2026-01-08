using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;

/// <summary>
/// Génère un donjon procédural complet.
/// </summary>
public class GenerateDungeonCommandHandler : IRequestHandler<GenerateDungeonCommand, Dungeon>
{
    private readonly IRepository<Dungeon> _dungeonRepository;
    private readonly ILogger<GenerateDungeonCommandHandler> _logger;

    public GenerateDungeonCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<GenerateDungeonCommandHandler> logger)
    {
        _dungeonRepository = unitOfWork.GetRepository<Dungeon>()
            ?? throw new ArgumentNullException(nameof(unitOfWork), "Repository Dungeon is null");
        _logger = logger;
    }

    public async Task<Dungeon> Handle(GenerateDungeonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Procedural Dungeon Generation...");

        try
        {
            var rng = new Random();
            var seed = rng.Next();
            
            // 1. Log du Seed : CRITIQUE pour reproduire les bugs de génération
            _logger.LogInformation("Dungeon Generation Configured with Seed: {Seed}", seed);

            var dungeon = new Dungeon { Seed = seed.ToString() };

            int levelsCount = rng.Next(10, 16);
            int totalRoomsCreated = 0;

            for (int levelIndex = 1; levelIndex <= levelsCount; levelIndex++)
            {
                var level = new Level { Number = levelIndex };
                int roomsCount = GetRoomCount(levelIndex, levelsCount, rng);

                for (int i = 0; i < roomsCount; i++)
                {
                    var room = CreateRoom(levelIndex, levelsCount, rng, i);
                    level.Rooms.Add(room);
                    totalRoomsCreated++;
                }

                dungeon.Levels.Add(level);
                
                // 2. Log de détail (Debug) : Pour suivre la boucle sans polluer la prod
                _logger.LogDebug("Generated Level {LevelNumber}/{TotalLevels} with {RoomCount} rooms", levelIndex, levelsCount, roomsCount);
            }
            
            // 3. Persistance
            await _dungeonRepository.AddAsync(dungeon);

            // 4. Log de résumé (Analytics)
            // Donne une vue d'ensemble de la taille des donjons générés par le système
            _logger.LogInformation("Dungeon {DungeonId} generated successfully. Stats: [Levels: {LevelCount}, Total Rooms: {TotalRooms}, Seed: {Seed}]", 
                dungeon.Id, dungeon.Levels.Count, totalRoomsCreated, seed);

            return dungeon;
        }
        catch (Exception ex)
        {
            // 5. Log d'erreur
            _logger.LogError(ex, "Procedural generation failed.");
            throw;
        }
    }

    private static int GetRoomCount(int levelIndex, int totalLevels, Random rng)
    {
        if (levelIndex == 1 || levelIndex == totalLevels)
            return 1;
        return rng.Next(1, 4); // 3 max
    }

    private static Room CreateRoom(int levelIndex, int totalLevels, Random rng, int i)
    {
        Room room;

        if (levelIndex == 1)
        {
            room = new CombatRoom { Type = RoomType.Entrance, MonsterNb = 0 };
        }
        else if (levelIndex == totalLevels)
        {
            room = new BossRoom { Type = RoomType.BossRoom, NameBoss = "Boss Final" };
        }
        else
        {
            int typeRoll = rng.Next(3);
            room = typeRoll switch
            {
                0 => new CombatRoom { Type = RoomType.CombatRoom, MonsterNb = rng.Next(1, 4) },
                1 => new TreasureRoom { Type = RoomType.TreasureRoom, Piece = rng.Next(1, 11) },
                _ => new TrapRoom { Type = RoomType.TrapRoom, Damage = rng.Next(1, 6) }
            };
        }

        room.Id = $"{levelIndex}{(char)('a' + i)}";
        room.Number = i + 1;
        room.Open = false;

        return room;
    }
}