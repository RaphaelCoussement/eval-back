using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DungeonCrawler_Game_Service.Application.Features.LinkRooms.Commands;

public class LinkRoomsCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<LinkRoomsCommandHandler> logger
    ) : IRequestHandler<LinkRoomsCommand, Dungeon>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();

    public async Task<Dungeon> Handle(LinkRoomsCommand request, CancellationToken cancellationToken)
    {
        // 1. Trace de la demande
        logger.LogInformation("Processing LinkRoomsCommand: Linking Room {FromRoomId} -> {ToRoomId} in Dungeon {DungeonId}", 
            request.FromRoomId, request.ToRoomId, request.DungeonId);

        try
        {
            var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);
            if (dungeon == null)
            {
                // 2. Log Warning (404)
                logger.LogWarning("LinkRooms failed: Dungeon {DungeonId} not found", request.DungeonId);
                throw new KeyNotFoundException($"Dungeon {request.DungeonId} not found");
            }
            
            // Vérification de l'existence du lien
            var linkExists = dungeon.Links.Any(l =>
                l.FromRoomId == request.FromRoomId && l.ToRoomId == request.ToRoomId);

            if (!linkExists)
            {
                // Action : Création du lien
                dungeon.Links.Add(new RoomLink
                {
                    FromRoomId = request.FromRoomId,
                    ToRoomId = request.ToRoomId
                });

                await _dungeonRepository.UpdateAsync(dungeon);
                
                // 3. Log de modification effective
                logger.LogInformation("Successfully created new link: {FromRoomId} <-> {ToRoomId}", request.FromRoomId, request.ToRoomId);
            }
            else
            {
                // 4. Log informatif (Idempotence)
                // Utile pour savoir que la requête a réussi mais n'a rien changé car le travail était déjà fait
                logger.LogInformation("Link between {FromRoomId} and {ToRoomId} already exists. No database update required.", 
                    request.FromRoomId, request.ToRoomId);
            }

            return dungeon;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            // 5. Log d'erreur système
            logger.LogError(ex, "System error while linking rooms in Dungeon {DungeonId}", request.DungeonId);
            throw;
        }
    }
}