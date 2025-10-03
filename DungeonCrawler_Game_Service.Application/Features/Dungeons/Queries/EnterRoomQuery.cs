using DungeonCrawler_Game_Service.Domain.Models;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;

/// <summary>
/// Query pour entrer dans une salle spécifique d'un donjon.
/// </summary>
public class EnterRoomQuery : IRequest<RoomProgress>
{
    /// <summary>
    /// DonjonId du donjon concerné récupéré dans le body de la requête
    /// </summary>
    public string DungeonId { get; set; } = string.Empty;
    
    /// <summary>
    /// NextRoomId de la salle à entrerr récupéré dans le body de la requête
    /// </summary>
    public string NextRoomId { get; set; } = string.Empty;
    
    
}