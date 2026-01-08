using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Queries;

public class GetCharacterByIdQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCharacterByIdQueryHandler> logger
    ) : IRequestHandler<GetCharacterByIdQuery, Character>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    /// <summary>
    /// Handler pour récupérer un personnage par son identifiant.
    /// </summary>
    public async Task<Character> Handle(GetCharacterByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Log contextuel (utile pour tracer une requête HTTP spécifique)
        logger.LogInformation("Fetching character details for ID {CharacterId}", request.Id);

        try
        {
            var character = await _characterRepository.GetByIdAsync(request.Id);
            
            if (character == null)
            {
                // 2. Log Warning : Distinction importante pour le monitoring (404 vs 500)
                logger.LogWarning("Character lookup failed: ID {CharacterId} not found in database", request.Id);
                
                // On lance l'exception qui sera transformée en 404 par ton middleware ou contrôleur
                throw new KeyNotFoundException($"Character with id {request.Id} not found");
            }
            
            // 3. Log de succès (Debug pour ne pas polluer si l'API est très sollicitée)
            logger.LogDebug("Successfully retrieved character {CharacterName} (Owner: {UserId})", character.Name, character.UserId);
            
            return character;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            // 4. Log d'erreur réel (Problème DB, Timeout, etc.)
            // On ignore KeyNotFoundException car on l'a déjà gérée proprement au-dessus
            logger.LogError(ex, "Database error while retrieving character {CharacterId}", request.Id);
            throw;
        }
    }
}