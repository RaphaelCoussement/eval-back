using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.EnterRoom.Queries;

public class EnterRoomQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<EnterRoomQuery, RoomProgress>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();

    public async Task<RoomProgress> Handle(EnterRoomQuery request, CancellationToken cancellationToken)
    {
        var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);
        if (dungeon == null)
        {
            throw new KeyNotFoundException($"Dungeon {request.DungeonId} not found");
        }
        var room = dungeon.Levels.SelectMany(l => l.Rooms).FirstOrDefault(r => r.Id == request.NextRoomId);
        var level = dungeon.Levels.FirstOrDefault(l => l.Rooms.Any(r => r.Id == request.NextRoomId));

        // On simule un "événement" dynamique
        var events = new List<string>();

        switch (room)
        {
            case null:
                events.Add("La salle n'existe pas");
                break;
            case CombatRoom combat when combat.MonsterNb > 0:
                events.Add($"Vous affrontez {combat.MonsterNb} monstre(s) !");
                break;
            case TreasureRoom treasure:
                events.Add($"Vous trouvez {treasure.Piece} pièces d'or !");
                break;
            case TrapRoom trap:
                events.Add($"Vous tombez dans un piège ! Dégâts : {trap.Damage}");
                break;
            case BossRoom boss:
                events.Add($"Vous rencontrez le {boss.NameBoss} !");
                break;
            default:
                events.Add("La salle est vide...");
                break;
        }

        return new RoomProgress
        {
            RoomId = room?.Id,
            Level = level?.Number ?? 0,
            Events = events
        };
    }
}