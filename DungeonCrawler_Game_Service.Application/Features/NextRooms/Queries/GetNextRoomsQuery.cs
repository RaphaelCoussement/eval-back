using DungeonCrawler_Game_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.NextRooms.Queries;

public record GetNextRoomsQuery(string DungeonId, string CurrentRoomId) : IRequest<List<Room>>;