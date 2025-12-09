
using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly;
using MediatR;
using Rebus.Bus;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Commands;

public class CreateCharacterCommandHandler(
    IUnitOfWork unitOfWork,
    IBus bus
    ) : IRequestHandler<CreateCharacterCommand, Character>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    /// <summary>
    /// Handler pour créer un nouveau personnage. Le personnage est ajouté à la base de données via le repository.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Character> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        // Crée une nouvelle instance de Character avec les données de la requête
        var character = new Character{ Name = request.Name, Class = (Classes)request.ClassCode, UserId = request.UserId };

        // Ajoute le personnage à la base de données via le repository
        await _characterRepository.AddAsync(character);
        
        // Publie un événement de création de personnage
        await bus.Publish(new CreateCharacterEvent
        {
            CharacterId = character.Id,
        });
        Console.WriteLine($"Created character {character.Name}");
        return character;
    }
}