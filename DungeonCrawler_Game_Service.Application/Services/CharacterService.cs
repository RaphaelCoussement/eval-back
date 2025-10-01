using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;

namespace DungeonCrawler_Game_Service.Application.Services;

public class CharacterService(
    IUnitOfWork unitOfWork
    ) : ICharacterService
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();
    public async Task<Character> CreateCharacterAsync(string name, Classes characterClass, string userId)
    {
        var character = new Character()
        {
            Name = name,
            Class = characterClass,
            UserId = userId
        };
        
        await _characterRepository.AddAsync(character);
        
        
        return character;
    }


}