using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;

public class LinkRoomsCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<LinkRoomsCommand, Dungeon>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();
    
    /// <summary>
    /// Handle pour lier deux salles dans un donjon.
    /// </summary>
    /// <returns>Le donjon</returns>
    public async Task<Dungeon> Handle(LinkRoomsCommand request, CancellationToken cancellationToken)
    {
        var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);

        // Vérifie si le lien existe déjà (dans les deux sens)
        var exists = dungeon.Links.Any(l =>
            (l.FromRoomId == request.FromRoomId && l.ToRoomId == request.ToRoomId) ||
            (l.FromRoomId == request.ToRoomId && l.ToRoomId == request.FromRoomId));
            
        // Si le lien n'existe pas, l'ajoute
        if (!exists)
        {
            // Ajoute le lien entre les deux salles
            dungeon.Links.Add(new RoomLink
            {
                FromRoomId = request.FromRoomId,
                ToRoomId = request.ToRoomId,
            });

            // Met à jour le donjon dans la base de données
            await _dungeonRepository.UpdateAsync(dungeon);
        }

        return dungeon;
    }
}