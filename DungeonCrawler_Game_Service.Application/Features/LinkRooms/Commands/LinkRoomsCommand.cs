using DungeonCrawler_Game_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.LinkRooms.Commands;

public record LinkRoomsCommand(string DungeonId, string FromRoomId, string ToRoomId) : IRequest<Dungeon>;