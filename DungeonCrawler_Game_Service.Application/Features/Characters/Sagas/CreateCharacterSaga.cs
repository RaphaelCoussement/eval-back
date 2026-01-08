using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly;
using DungeonCrawlerAssembly.Messages;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Sagas;

/// <summary>
/// Saga pour gérer la création de personnage de manière transactionnelle.
/// </summary>
public class CreateCharacterSaga : Saga<CreateCharacterSagaData>,
    IAmInitiatedBy<CreateCharacterEvent>,        
    IHandleMessages<CharacterCreationConfirmed>, 
    IHandleMessages<CharacterCreationFailed>     
{
    private readonly ILogger<CreateCharacterSaga> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCharacterSaga(
        ILogger<CreateCharacterSaga> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    protected override void CorrelateMessages(ICorrelationConfig<CreateCharacterSagaData> config)
    {
        config.Correlate<CreateCharacterEvent>(m => m.CharacterId, d => d.CharacterId);
        config.Correlate<CharacterCreationConfirmed>(m => m.CharacterId, d => d.CharacterId);
        config.Correlate<CharacterCreationFailed>(m => m.CharacterId, d => d.CharacterId);
    }

    /// <summary>
    /// Étape 1: Démarrage
    /// </summary>
    public async Task Handle(CreateCharacterEvent message)
    {
        // On initialise les données
        Data.CharacterId = message.CharacterId;
        Data.UserId = message.UserId;
        Data.CreatedAt = DateTime.UtcNow;
        
        // LOG : Début de la transaction distribuée
        // Important : On loggue le UserId pour pouvoir filtrer tous les logs d'un utilisateur spécifique dans OpenTelemetry
        _logger.LogInformation("Saga Initiated: Waiting for confirmation for Character {CharacterId} (User: {UserId})", 
            message.CharacterId, message.UserId);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Étape 2a: Succès
    /// </summary>
    public async Task Handle(CharacterCreationConfirmed message)
    {
        Data.IsCompleted = true;

        // LOG : Fin heureuse de la transaction
        _logger.LogInformation("Saga Completed Successfully: Character {CharacterId} confirmed by remote service. (Msg: {RemoteMessage})", 
            message.CharacterId, message.Message);

        MarkAsComplete();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Étape 2b: Échec et Compensation
    /// </summary>
    public async Task Handle(CharacterCreationFailed message)
    {
        Data.IsFailed = true;
        Data.FailureReason = message.Reason;

        // LOG : Avertissement business (Transaction annulée)
        _logger.LogWarning("Saga Rollback Triggered: Creation failed for Character {CharacterId}. Reason: {Reason}. Starting compensation...", 
            message.CharacterId, message.Reason);

        // Exécution de la compensation
        await CompensateCharacterCreation(message.CharacterId);

        MarkAsComplete();
    }

    /// <summary>
    /// Compensation: Suppression locale
    /// </summary>
    private async Task CompensateCharacterCreation(string characterId)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<Character>();
            var character = await repository.GetByIdAsync(characterId);
            
            if (character != null)
            {
                await repository.RemoveAsync(characterId);
                
                // LOG : Confirmation que le nettoyage a fonctionné
                _logger.LogInformation("Compensation Success: Local character {CharacterId} has been deleted to match remote failure.", characterId);
            }
            else
            {
                // Cas étrange mais pas bloquant : on devait supprimer un truc qui n'existe déjà plus
                _logger.LogWarning("Compensation: Character {CharacterId} was already missing from DB.", characterId);
            }
        }
        catch (Exception ex)
        {
            // LOG : ERREUR CRITIQUE (DATA INCONSISTENCY)
            // C'est le pire scénario : l'autre service a dit "non", mais nous on a échoué à supprimer le perso.
            // Le perso existe donc chez nous mais pas chez eux. Il faut une alerte ici.
            _logger.LogError(ex, "CRITICAL COMPENSATION FAILURE: Failed to delete Character {CharacterId}. Database is now in inconsistent state!", characterId);
            
        }
    }
}