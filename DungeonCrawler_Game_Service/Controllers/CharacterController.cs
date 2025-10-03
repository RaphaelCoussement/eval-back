using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CharacterController(
    IMediator mediator
    ) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Création d'un personnage
    /// </summary>
    /// <returns>L'état de la requête avec le character</returns>

    [HttpPost]
    public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}