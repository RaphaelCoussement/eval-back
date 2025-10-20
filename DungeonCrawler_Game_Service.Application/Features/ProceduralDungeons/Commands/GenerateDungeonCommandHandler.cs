using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;

/// <summary>
/// Génère un donjon procédural complet.
/// Chaque étage contient un certain nombre de salles, et les liens sont créés
/// de manière logique (aucun chemin forcé, exploration libre).
/// </summary>
public class GenerateDungeonCommandHandler : IRequestHandler<GenerateDungeonCommand, Dungeon>
{
    private readonly IRepository<Dungeon> _dungeonRepository;

    public GenerateDungeonCommandHandler(IUnitOfWork unitOfWork)
    {
        _dungeonRepository = unitOfWork.GetRepository<Dungeon>()
            ?? throw new ArgumentNullException(nameof(unitOfWork), "Repository Dungeon is null");
    }

    public async Task<Dungeon> Handle(GenerateDungeonCommand request, CancellationToken cancellationToken)
    {
        var rng = new Random();
        var seed = rng.Next();

        var dungeon = new Dungeon { Seed = seed.ToString() };

        int levelsCount = rng.Next(10, 16);

        for (int levelIndex = 1; levelIndex <= levelsCount; levelIndex++)
        {
            var level = new Level { Number = levelIndex };
            int roomsCount = GetRoomCount(levelIndex, levelsCount, rng);

            for (int i = 0; i < roomsCount; i++)
            {
                var room = CreateRoom(levelIndex, levelsCount, rng, i);
                level.Rooms.Add(room);
            }

            dungeon.Levels.Add(level);
        }
        
        await _dungeonRepository.AddAsync(dungeon);

        return dungeon;
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