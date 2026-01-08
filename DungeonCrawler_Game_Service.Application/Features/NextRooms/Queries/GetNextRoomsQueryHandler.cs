using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DungeonCrawler_Game_Service.Application.Features.NextRooms.Queries;

public class GetNextRoomsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetNextRoomsQueryHandler> logger
    ) : IRequestHandler<GetNextRoomsQuery, List<Room>>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();

    public async Task<List<Room>> Handle(GetNextRoomsQuery request, CancellationToken cancellationToken)
    {
        // 1. Trace de début
        logger.LogInformation("Fetching next rooms suggestions for Dungeon {DungeonId} from Room {CurrentRoomId}", 
            request.DungeonId, request.CurrentRoomId);

        try
        {
            var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);
            if (dungeon == null)
            {
                logger.LogWarning("GetNextRooms failed: Dungeon {DungeonId} not found", request.DungeonId);
                throw new KeyNotFoundException($"Dungeon {request.DungeonId} not found");
            }
            
            // Recherche du niveau actuel
            var currentLevel = dungeon.Levels.FirstOrDefault(l => l.Rooms.Any(r => r.Id == request.CurrentRoomId));
            
            if (currentLevel == null) 
            {
                // 2. Cas étrange : L'ID de la salle existe peut-être, mais n'est pas dans CE donjon
                logger.LogWarning("Navigation mismatch: Room {CurrentRoomId} does not exist inside Dungeon {DungeonId}", 
                    request.CurrentRoomId, request.DungeonId);
                return new List<Room>();
            }

            // Récupère les salles du niveau suivant
            var nextLevel = dungeon.Levels.FirstOrDefault(l => l.Number == currentLevel.Number + 1);
            
            if (nextLevel == null) 
            {
                // 3. Cas métier normal : Fin du donjon atteinte
                logger.LogInformation("No next level found for Dungeon {DungeonId} after Level {CurrentLevelNumber}. Player reached the end?", 
                    request.DungeonId, currentLevel.Number);
                return new List<Room>();
            }

            // 4. Succès
            logger.LogDebug("Found {RoomCount} available rooms in Level {NextLevelNumber}", nextLevel.Rooms.Count, nextLevel.Number);
            return nextLevel.Rooms;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            // 5. Erreur technique
            logger.LogError(ex, "System error while fetching next rooms for Dungeon {DungeonId}", request.DungeonId);
            throw;
        }
    }
}