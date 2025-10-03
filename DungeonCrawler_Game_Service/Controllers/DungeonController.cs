using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;
using DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DungeonController(
    IMediator mediator
    ) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    ///  Génération d'un donjon
    /// </summary>
    /// <returns>L'état de la requête</returns>
    [HttpPost]
    public async Task<IActionResult> GenerateDungeon(GenerateDungeonCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    ///  Récupère les salles suivantes possibles depuis une salle donnée dans un donjon.
    /// </summary>
    /// <returns>La liste des rooms suivante</returns>
    [HttpGet("{dungeonId}/room/{roomId}/next")]
    public async Task<IActionResult> GetNextRooms(string dungeonId, string roomId)
    {
        var response = await _mediator.Send(new GetNextRoomsQuery(dungeonId, roomId));
        return Ok(response);
    }

    /// <summary>
    /// Permet d'entrer dans une salle spécifique d'un donjon.
    /// </summary>
    /// <param name="dungeonId">Id du donjon concerné</param>
    /// <returns>Létat de la requete</returns>
    [HttpPost("{dungeonId}/enter")]
    public async Task<IActionResult> EnterRoom(string dungeonId, [FromBody] EnterRoomQuery query)
    {
        // Assigne le dungeonId de la route au query
        query.DungeonId = dungeonId;
        var response = await _mediator.Send(query);
        return Ok(response);
    }
}