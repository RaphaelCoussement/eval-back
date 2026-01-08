using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Commands;

/// <summary>
///  Handler pour équiper une skin à un personnage
/// </summary>
public class EquipSkinCommandHandler(
    IUnitOfWork unitOfWork,
    IBus bus,
    ILogger<EquipSkinCommandHandler> logger
) : IRequestHandler<EquipSkinCommand, bool>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    public async Task<bool> Handle(EquipSkinCommand request, CancellationToken cancellationToken)
    {
        // 1. Log de début avec les paramètres
        logger.LogInformation("Processing EquipSkinCommand: Equipping Skin {SkinId} for Character {CharacterId}", 
            request.SkinId, request.CharacterId);

        try
        {
            // Récupère le personnage
            var character = await _characterRepository.GetByIdAsync(request.CharacterId);
            
            if (character == null)
            {
                // 2. Log Warning : Ce n'est pas une erreur système, mais une erreur "logique" (404)
                logger.LogWarning("EquipSkin failed: Character {CharacterId} not found in database", request.CharacterId);
                throw new Exception($"Character {request.CharacterId} not found");
            }

            // 3. Log de changement d'état (Avant -> Après)
            // Très utile pour savoir ce qui a été remplacé
            logger.LogInformation("Updating ActiveSkinId for Character {CharacterId}: {OldSkinId} -> {NewSkinId}", 
                character.Id, character.ActiveSkinId, request.SkinId);

            // Mise à jour
            character.ActiveSkinId = request.SkinId;
            await _characterRepository.UpdateAsync(character);
            
            // 4. Log de succès
            logger.LogInformation("Skin {SkinId} successfully equipped for Character {CharacterId}", request.SkinId, character.Id);
            
            return true;
        }
        catch (Exception ex)
        {
            // 5. Log d'erreur système (ex: DB timeout)
            // On exclut l'erreur "Character not found" qu'on a déjà loggée en warning juste au-dessus
            if (ex.Message != $"Character {request.CharacterId} not found")
            {
                logger.LogError(ex, "System error while equipping skin {SkinId} for character {CharacterId}", request.SkinId, request.CharacterId);
            }
            
            throw; // Relance pour gestion HTTP
        }
    }
}