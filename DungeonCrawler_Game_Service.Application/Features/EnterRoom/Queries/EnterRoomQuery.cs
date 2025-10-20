using DungeonCrawler_Game_Service.Domain.Models;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.EnterRoom.Queries;

/// <summary>
/// Query pour entrer dans une salle spécifique d'un donjon.
/// </summary>
public record EnterRoomQuery(string DungeonId, string NextRoomId) : IRequest<RoomProgress>;