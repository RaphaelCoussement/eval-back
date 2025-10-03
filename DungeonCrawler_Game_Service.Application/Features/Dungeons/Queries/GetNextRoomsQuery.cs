using DungeonCrawler_Game_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;

/// <summary>
/// Query pour obtenir les salles accessibles depuis la salle actuelle d'un donjon.
/// </summary>
/// <param name="DungeonId">Id du donjon concerné</param>
/// <param name="CurrentRoomId">Id de la salle actuelle</param>
public record GetNextRoomsQuery(string DungeonId, string CurrentRoomId) : IRequest<List<Room>>;