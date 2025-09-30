using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Application.Services;
using DungeonCrawler_Game_Service.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DefaultNamespace;

[ApiController]
public class CharacterController(ICharacterService characterService) : ControllerBase
{
    /// <summary>
    /// Création d'un personnage
    /// </summary>
    /// <param name="request">La requête</param>
    /// <returns>Le personnage</returns>
    public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterRequest request)
    {
        // Validation des données d'entrée
        if (string.IsNullOrEmpty(request.Name) || (request.ClassCode <= 0 || request.ClassCode > 3))
        {
            return BadRequest("Invalid character data.");
        }

        // Creation du personnage
        var character = await characterService.CreateCharacterAsync(request.Name, (Classes)request.ClassCode);
        

        return Ok(character);
    }
    
}