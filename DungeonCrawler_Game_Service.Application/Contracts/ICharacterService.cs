using DefaultNamespace;

namespace DungeonCrawler_Game_Service.Application.Contracts;

public interface ICharacterService
{
    /// <summary>
    /// Crée un personnage
    /// </summary>
    /// <param name="name">Nom du personnage</param>
    /// <param name="characterClass">Classe du personnage</param>
    /// <param name="userId">Identifiant de l'utilisateur propriétaire du personnage</param>
    /// <returns></returns>
    Task<Character> CreateCharacterAsync(string name, Classes characterClass, string userId);
    
}