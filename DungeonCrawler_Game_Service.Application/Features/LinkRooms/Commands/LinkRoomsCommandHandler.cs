using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.LinkRooms.Commands;

public class LinkRoomsCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<LinkRoomsCommand, Dungeon>
{
    private readonly IRepository<Dungeon> _dungeonRepository = unitOfWork.GetRepository<Dungeon>();

    public async Task<Dungeon> Handle(LinkRoomsCommand request, CancellationToken cancellationToken)
    {
        var dungeon = await _dungeonRepository.GetByIdAsync(request.DungeonId);
        if (dungeon == null)
        {
            throw new KeyNotFoundException($"Dungeon {request.DungeonId} not found");
        }
        
        var linkExists = dungeon.Links.Any(l =>
            l.FromRoomId == request.FromRoomId && l.ToRoomId == request.ToRoomId);

        if (!linkExists)
        {
            dungeon.Links.Add(new RoomLink
            {
                FromRoomId = request.FromRoomId,
                ToRoomId = request.ToRoomId
            });

            await _dungeonRepository.UpdateAsync(dungeon);
        }

        return dungeon;
    }
}