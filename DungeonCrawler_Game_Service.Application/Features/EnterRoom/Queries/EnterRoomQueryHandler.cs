using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DungeonCrawler_Game_Service.Application.Features.EnterRoom.Queries;

public class EnterRoomQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<EnterRoomQueryHandler> logger
    ) : IRequestHandler<EnterRoomQuery, RoomProgress>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();

    public async Task<RoomProgress> Handle(EnterRoomQuery request, CancellationToken cancellationToken)
    {
        // 1. Trace de début avec contexte
        logger.LogInformation("Processing EnterRoomQuery: Dungeon {DungeonId}, Target Room {RoomId}", 
            request.DungeonId, request.NextRoomId);

        try
        {
            var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);
            if (dungeon == null)
            {
                // 2. Log Warning pour Dungeon introuvable
                logger.LogWarning("EnterRoom failed: Dungeon {DungeonId} not found", request.DungeonId);
                throw new KeyNotFoundException($"Dungeon {request.DungeonId} not found");
            }

            // Recherche de la salle et du niveau
            var room = dungeon.Levels.SelectMany(l => l.Rooms).FirstOrDefault(r => r.Id == request.NextRoomId);
            var level = dungeon.Levels.FirstOrDefault(l => l.Rooms.Any(r => r.Id == request.NextRoomId));

            // On simule un "événement" dynamique
            var events = new List<string>();

            // 3. LOGS MÉTIER (Game Analytics)
            // Ici, on loggue ce qui se passe réellement dans le jeu.
            // Ces logs permettront de savoir quel type de salle est le plus fréquenté.
            switch (room)
            {
                case null:
                    logger.LogWarning("Room lookup failed: Room ID {RoomId} does not exist in Dungeon {DungeonId}", request.NextRoomId, request.DungeonId);
                    events.Add("La salle n'existe pas");
                    break;

                case CombatRoom combat when combat.MonsterNb > 0:
                    logger.LogInformation("Event: Combat triggered. Monsters: {MonsterCount}", combat.MonsterNb);
                    events.Add($"Vous affrontez {combat.MonsterNb} monstre(s) !");
                    break;

                case TreasureRoom treasure:
                    logger.LogInformation("Event: Treasure found. Amount: {GoldAmount}", treasure.Piece);
                    events.Add($"Vous trouvez {treasure.Piece} pièces d'or !");
                    break;

                case TrapRoom trap:
                    logger.LogInformation("Event: Trap triggered. Damage: {DamageAmount}", trap.Damage);
                    events.Add($"Vous tombez dans un piège ! Dégâts : {trap.Damage}");
                    break;

                case BossRoom boss:
                    logger.LogInformation("Event: BOSS Encounter! Boss: {BossName}", boss.NameBoss);
                    events.Add($"Vous rencontrez le {boss.NameBoss} !");
                    break;

                default:
                    logger.LogDebug("Event: Entered empty room {RoomId}", room.Id);
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
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            // 4. Log d'erreur technique
            logger.LogError(ex, "System error while entering room {RoomId} in dungeon {DungeonId}", request.NextRoomId, request.DungeonId);
            throw;
        }
    }
}