using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly;
using DungeonCrawlerAssembly.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Commands;

public class CreateCharacterCommandHandler(
    IUnitOfWork unitOfWork,
    IBus bus,
    ILogger<CreateCharacterCommandHandler> logger
    ) : IRequestHandler<CreateCharacterCommand, Character>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    /// <summary>
    /// Handler pour créer un nouveau personnage.
    /// </summary>
    public async Task<Character> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        // 1. Log de début de traitement (Contextualisation)
        logger.LogInformation("Starting CreateCharacterCommand for User {UserId} with Name {CharacterName} and ClassCode {ClassCode}", 
            request.UserId, request.Name, request.ClassCode);

        try
        {
            // Création de l'entité
            var character = new Character
            { 
                Name = request.Name, 
                Class = (Classes)request.ClassCode, 
                UserId = request.UserId 
            };

            // 2. Persistance BDD
            await _characterRepository.AddAsync(character);
            logger.LogDebug("Character {CharacterId} successfully persisted in database", character.Id);
            
            // 3. Publication de l'événement (Point critique d'intégration)
            logger.LogInformation("Publishing CreateCharacterEvent for Character {CharacterId}...", character.Id);
            
            await bus.Publish(new CreateCharacterEvent
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            });

            // 4. Log de fin de succès
            logger.LogInformation("Successfully created character {CharacterName} ({CharacterId}) for User {UserId}", 
                character.Name, character.Id, character.UserId);
            
            return character;
        }
        catch (Exception ex)
        {
            // 5. Log d'erreur structuré
            // OpenTelemetry va capturer la StackTrace et l'associer à ces attributs (UserId, Name)
            logger.LogError(ex, "Failed to create character {CharacterName} for User {UserId}. Error: {ErrorMessage}", 
                request.Name, request.UserId, ex.Message);
            
            // On relance l'exception pour que le contrôleur renvoie une 500
            throw; 
        }
    }
}