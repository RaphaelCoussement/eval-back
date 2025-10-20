using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.NextRooms.Queries;

public class GetNextRoomsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetNextRoomsQuery, List<Room>>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();

    public async Task<List<Room>> Handle(GetNextRoomsQuery request, CancellationToken cancellationToken)
    {
        var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);
        if (dungeon == null)
        {
            throw new KeyNotFoundException($"Dungeon {request.DungeonId} not found");
        }
        
        var currentLevel = dungeon.Levels.FirstOrDefault(l => l.Rooms.Any(r => r.Id == request.CurrentRoomId));
        if (currentLevel == null) return new List<Room>();

        // Récupère les salles du niveau suivant
        var nextLevel = dungeon.Levels.FirstOrDefault(l => l.Number == currentLevel.Number + 1);
        if (nextLevel == null) return new List<Room>();

        return nextLevel.Rooms;
    }
}