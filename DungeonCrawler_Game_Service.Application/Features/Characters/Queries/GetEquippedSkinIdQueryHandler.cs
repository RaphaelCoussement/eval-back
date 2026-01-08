using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Queries;

public class GetEquippedSkinIdQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetEquippedSkinIdQueryHandler> logger
) : IRequestHandler<GetEquippedSkinIdQuery, string>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    /// <summary>
    /// Handler pour récupérer l'identifiant du skin équipé d'un personnage.
    /// </summary>
    public async Task<string> Handle(GetEquippedSkinIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Trace de début
        logger.LogInformation("Fetching equipped skin ID for Character {CharacterId}", request.CharacterId);

        try
        {
            var character = await _characterRepository.GetByIdAsync(request.CharacterId);
            
            if (character == null)
            {
                // 2. Log Warning pour le cas "Not Found"
                logger.LogWarning("Cannot retrieve skin: Character {CharacterId} not found in database", request.CharacterId);
                throw new KeyNotFoundException($"Character with id {request.CharacterId} not found");
            }
            
            // 3. Log de détail (Debug)
            // On gère le cas où ActiveSkinId serait null pour l'affichage
            var skinDisplay = character.ActiveSkinId ?? "None";
            logger.LogDebug("Character {CharacterId} is currently equipping skin: {SkinId}", character.Id, skinDisplay);
            
            return character.ActiveSkinId;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            // 4. Log d'erreur système uniquement (Database down, etc.)
            logger.LogError(ex, "System error while fetching skin for Character {CharacterId}", request.CharacterId);
            throw;
        }
    }
}