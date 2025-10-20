using DungeonCrawler_Game_Service.Application.Features.EnterRoom.Queries;
using DungeonCrawler_Game_Service.Application.Features.LinkRooms.Commands;
using DungeonCrawler_Game_Service.Application.Features.NextRooms.Queries;
using DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;
using DungeonCrawler_Game_Service.Models.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class DungeonController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    ///  Génération d'un donjon procédural.
    /// </summary>
    [HttpPost]
    [Route("")]
    public async Task<ActionResult<Dungeon>> GenerateDungeon()
    {
        var dungeon = await _mediator.Send(new GenerateDungeonCommand());
        return Ok(dungeon);
    }

    /// <summary>
    ///  Entrer dans une salle du donjon.
    /// </summary>
    [HttpPost]
    [Route("enter")]
    public async Task<ActionResult<RoomProgress>> EnterRoom([FromBody] EnterRoomRequest request)
    {
        var result = await _mediator.Send(new EnterRoomQuery(request.DungeonId, request.NextRoomId));
        return Ok(result);
    }

    /// <summary>
    ///  Récupère les salles accessibles depuis la salle actuelle.
    /// </summary>
    [HttpGet]
    [Route("{dungeonId}/next")]
    public async Task<ActionResult<List<Room>>> GetNextRooms([FromQuery] string currentRoomId, [FromRoute] string dungeonId)
    {
        var result = await _mediator.Send(new GetNextRoomsQuery(dungeonId, currentRoomId));
        return Ok(result);
    }

    /// <summary>
    ///  Lie deux salles dans le donjon.
    /// </summary>
    [HttpPost]
    [Route("link")]
    public async Task<ActionResult<Dungeon>> LinkRooms([FromBody] LinkRoomsRequest request)
    {
        var result = await _mediator.Send(new LinkRoomsCommand(request.DungeonId, request.FromRoomId, request.ToRoomId));
        return Ok(result);
    }
}