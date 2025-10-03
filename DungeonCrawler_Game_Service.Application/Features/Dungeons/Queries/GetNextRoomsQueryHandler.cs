using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;

public class GetNextRoomsQueryHandler(
    IUnitOfWork unitOfWork
    ) : IRequestHandler<GetNextRoomsQuery , List<Room>>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();
    
    /// <summary>
    /// Handle pour obtenir les salles accessibles depuis la salle actuelle d'un donjon.
    /// </summary>
    /// <returns>La liste des salles suivantes accessibles</returns>
    public async Task<List<Room>> Handle(GetNextRoomsQuery request, CancellationToken cancellationToken)
    {
        // Récupère le donjon
        var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);

        // Trouve la salle cliquée
        var currentRoom = dungeon.Levels.SelectMany(l => l.Rooms).FirstOrDefault(r => r.Id == request.CurrentRoomId);
        
        // Retourne toutes les salles dont le NextRoomId correspond à currentRoom.NextRoomId**
        var nextRooms =  dungeon.Levels
            .SelectMany(l => l.Rooms)
            .Where(r => r.Id == currentRoom.NextRoomId || r.Id.StartsWith(currentRoom.NextRoomId?.Substring(0, 1) ?? ""))
            .ToList();

        return nextRooms;
    }
}