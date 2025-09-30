using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;

namespace DungeonCrawler_Game_Service.Application.Services;

public class CharacterService(
    IUnitOfWork unitOfWork
    ) : ICharacterService
{
    private readonly IRepository<CharacterDao> _characterRepository = unitOfWork.GetRepository<CharacterDao>();
    public async Task<Character> CreateCharacterAsync(string name, Classes characterClass)
    {
        var character = new CharacterDao()
        {
            Name = name,
            Class = characterClass
        };
        
        await _characterRepository.AddAsync(character);
        
        
        // A remplacer par un truc propre
        return new Character()
        {
            Id = character.Id,
            Name = character.Name,
            Class = character.Class
        };
    }
    
    
}