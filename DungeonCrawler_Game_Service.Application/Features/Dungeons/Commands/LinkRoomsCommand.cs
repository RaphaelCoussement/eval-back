using DungeonCrawler_Game_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;

public class LinkRoomsCommand : IRequest<Dungeon>
{
    /// <summary>
    /// Id du donjon concerné récupéré dans le body de la requête
    /// </summary>
    public string DungeonId { get; set; } = string.Empty;
    /// <summary>
    /// Id de la salle de départ récupéré dans le body de la requête
    /// </summary>
    public string FromRoomId { get; set; } = string.Empty;
    /// <summary>
    /// Id de la salle d'arrivée récupéré dans le body de la requête
    /// </summary>
    public string ToRoomId { get; set; } = string.Empty;
    
}