using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;


public class EnterRoomQueryHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<EnterRoomQuery , RoomProgress>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();
    
    /// <summary>
    /// Gère l'entrée dans une salle spécifique d'un donjon.
    /// </summary>
    /// <returns>La salle entrée</returns>
    public async Task<RoomProgress> Handle(EnterRoomQuery request, CancellationToken cancellationToken)
    {
        // Récupère le donjon
        var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);
        
        // Récupère la salle entrée
        var room = dungeon.Levels.SelectMany(l => l.Rooms).FirstOrDefault(r => r.Id == request.NextRoomId);
        
        // Trouve le niveau de la salle
        var level = dungeon.Levels.FirstOrDefault(l => l.Rooms.Any(r => r.Id == request.NextRoomId));
        
        return new RoomProgress
        {
            RoomId = room.Id,
            Level = level?.Number ?? 0,
            Events = new List<string> { "monster", "loot" } // Placeholder pour test
        };
    }
}