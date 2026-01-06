using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Application.Features.Characters.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
//[Authorize]
[Route("game/[controller]")]
public class CharacterController(
    IMediator mediator
    ) : ControllerBase
{
    /// <summary>
    /// Création d'un personnage
    /// </summary>
    /// <returns>L'état de la requête avec le character</returns>

    [HttpPost]
    public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterCommand command)
    {
        var response = await mediator.Send(command);
        return Ok(response);
    }
    
    /// <summary>
    /// Récupère un personnage via son Id
    /// </summary>
    /// <param name="id">Id du personnage</param>
    /// <returns>Le personnage</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCharacterById([FromRoute] string id)
    {
        var query = new GetCharacterByIdQuery(id);
        var response = await mediator.Send(query);
        return Ok(response);
    }
    
    /// <summary>
    /// Equipe une skin à un personnage
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("skin")]
    public async Task<IActionResult> EquipSkin([FromBody] EquipSkinCommand command)
    {
        var response = await mediator.Send(command);
        return Ok(response);
    }
    
    /// <summary>
    /// Récupère l'ID de la skin équipée par un personnage
    /// </summary>
    /// <param name="characterId"></param>
    /// <returns></returns>
    [HttpGet("skin/{characterId}")]
    public async Task<IActionResult> GetEquippedSkinId([FromRoute] string characterId)
    {
        var query = new GetEquippedSkinIdQuery(characterId);
        var response = await mediator.Send(query);
        return Ok(response);
    }
    
}