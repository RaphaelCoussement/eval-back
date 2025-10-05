using DefaultNamespace;
using DungeonCrawler_Game_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;

public class GenerateDungeonCommandHandler : IRequestHandler<GenerateDungeonCommand, Dungeon>
{
    /// <summary>
    /// Handler pour générer un nouveau donjon à partir d'une seed.
    /// </summary>
    /// <returns></returns>
    public async Task<Dungeon> GenerateDungeon()
    {
        var seed = new Random().Next();
        var generation = new Random(seed);

        var levels = GenerateLevels(generation);

        return new Dungeon
        {
            Seed = seed.ToString(),
            Levels = levels,
        };
    }

    private static List<Level> GenerateLevels(Random generation)
    {
        int levelsNb = generation.Next(15, 21);
        var levels = new List<Level>();

        for (int x = 0; x < levelsNb; x++)
        {
            List<Room> rooms;

            if (x == levelsNb - 1) // Dernier niveau = Boss
            {
                rooms = new List<Room>
                {
                    new BossRoom
                    {
                        Number = 1,
                        Open = false,
                        NameBoss = "Boss"
                    }
                };
            }
            else
            {
                rooms = GenerateRooms(generation);
            }

            levels.Add(new Level
            {
                Number = x,
                Rooms = rooms
            });
        }

        return levels;
    }

    private static List<Room> GenerateRooms(Random generation)
    {
        int roomsNb = generation.Next(1, 4);
        var rooms = new List<Room>();

        for (int i = 0; i < roomsNb; i++)
        {
            rooms.Add(CreateRoom(generation, i));
        }

        return rooms;
    }

    private static Room CreateRoom(Random generation, int number)
    {
        int roomType = generation.Next(1, 4);

        return roomType switch
        {
            (int)RoomType.CombatRoom => new CombatRoom
            {
                Number = number,
                Open = false,
                MonsterNb = generation.Next(1, 4)
            },
            (int)RoomType.TreasureRoom => new TreasureRoom
            {
                Number = number,
                Open = false,
                Piece = generation.Next(1, 11)
            },
            _ => new TrapRoom
            {
                Number = number,
                Open = false,
                Degat = generation.Next(1, 6)
            }
        };
    }

    public Task<Dungeon> Handle(GenerateDungeonCommand request, CancellationToken cancellationToken)
    {
        return GenerateDungeon();
    }
}
