using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Models.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CharacterController : ControllerBase
{
    private readonly ICharacterService _characterService;

    public CharacterController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    /// <summary>
    /// Création d'un personnage
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Ok), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || (request.ClassCode <= 0 || request.ClassCode > 3))
        {
            return BadRequest(new ErrorResponse
            {
                Code = "INVALID_CHARACTER",
                Message = "Invalid character data."
            });
        }

        try
        {
            var character = await _characterService.CreateCharacterAsync(
                request.Name, 
                (Classes)request.ClassCode, 
                request.UserId
            );

            return Ok(character);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse
            {
                Code = "CHARACTER_CREATION_FAILED",
                Message = ex.Message
            });
        }
    }
}