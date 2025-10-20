using DungeonCrawler_Game_Service.Application.Features.EnterRoom.Queries;
using DungeonCrawler_Game_Service.Application.Features.LinkRooms.Commands;
using DungeonCrawler_Game_Service.Application.Features.NextRooms.Queries;
using DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Domain.Models;
using DungeonCrawler_Game_Service.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DungeonController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///  Génération d'un donjon procédural.
    /// </summary>
    [HttpPost]
    [Route("generate")]
    public async Task<ActionResult<Dungeon>> GenerateDungeon()
    {
        var dungeon = await mediator.Send(new GenerateDungeonCommand());
        return Ok(dungeon);
    }

    /// <summary>
    ///  Entrer dans une salle du donjon.
    /// </summary>
    [HttpPost]
    [Route("enter")]
    public async Task<ActionResult<RoomProgress>> EnterRoom([FromBody] EnterRoomQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    ///  Récupère les salles accessibles depuis la salle actuelle.
    /// </summary>
    [HttpGet]
    [Route("{dungeonId:int}/next")]
    public async Task<ActionResult<List<Room>>> GetNextRooms([FromQuery] string currentRoomId, [FromRoute] string dungeonId)
    {
        var result = await mediator.Send(new GetNextRoomsQuery(dungeonId, currentRoomId));
        return Ok(result);
    }

    /// <summary>
    ///  Lie deux salles dans le donjon.
    /// </summary>
    [HttpPost]
    [Route("link")]
    public async Task<ActionResult<Dungeon>> LinkRooms([FromBody] LinkRoomsCommand request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }
}