using DungeonCrawler_Game_Service.Application.Contracts;
//using DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;
//using DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;
using DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Models;
using DungeonCrawler_Game_Service.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
//[Authorize]
public class DungeonController(
    IMediator mediator
    ) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    ///  Génération d'un donjon
    /// </summary>
    /// <returns>L'état de la requête</returns>
    [HttpPost(ApiRoutes.Dungeon)]
    public async Task<ActionResult<Dungeon>> GenerateDungeon()
    {
        return Ok(_mediator.Send(new GenerateDungeonCommand()));
    }
}