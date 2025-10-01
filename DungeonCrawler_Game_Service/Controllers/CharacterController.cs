using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
public class CharacterController(ICharacterService characterService) : ControllerBase
{
    /// <summary>
    /// Création d'un personnage
    /// </summary>
    /// <param name="request">La requête</param>
    /// <returns>Le personnage</returns>
    [HttpPost("api/character")]
    public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterRequest request)
    {
        Console.WriteLine("Tentative de création de personnage...");
        // Validation des données d'entrée
        if (string.IsNullOrEmpty(request.Name) || (request.ClassCode <= 0 || request.ClassCode > 3))
        {
            return BadRequest("Invalid character data.");
        }
        
        // Création du personnage
        var character = await characterService.CreateCharacterAsync(request.Name, (Classes)request.ClassCode, request.UserId);
        

        // Retourner le personnage créé
        return Ok(character);
    }
    
}