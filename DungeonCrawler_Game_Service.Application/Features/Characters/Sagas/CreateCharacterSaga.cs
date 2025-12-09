using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly;
using DungeonCrawlerAssembly.Messages;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;
namespace DungeonCrawler_Game_Service.Application.Features.Characters.Sagas;

/// <summary>
/// Saga pour gérer la création de personnage de manière transactionnelle entre microservices.
/// 
/// Flow:
/// 1. Reçoit CreateCharacterEvent (publié par le CommandHandler)
/// 2. Attend CharacterCreationConfirmed OU CharacterCreationFailed de l'autre microservice
/// 3. Si échec → exécute la compensation (supprime le personnage)
/// </summary>
public class CreateCharacterSaga : Saga<CreateCharacterSagaData>,
    IAmInitiatedBy<CreateCharacterEvent>,        // Démarre la saga
    IHandleMessages<CharacterCreationConfirmed>, // Succès depuis l'autre service
    IHandleMessages<CharacterCreationFailed>     // Échec depuis l'autre service
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

    /// <summary>
    /// Configure comment corréler les messages à cette instance de saga.
    /// Tous les messages sont corrélés par CharacterId.
    /// </summary>
    protected override void CorrelateMessages(ICorrelationConfig<CreateCharacterSagaData> config)
    {
        // Le CharacterId est notre clé de corrélation
        config.Correlate<CreateCharacterEvent>(m => m.CharacterId, d => d.CharacterId);
        config.Correlate<CharacterCreationConfirmed>(m => m.CharacterId, d => d.CharacterId);
        config.Correlate<CharacterCreationFailed>(m => m.CharacterId, d => d.CharacterId);
    }

    /// <summary>
    /// Étape 1: La saga démarre quand on reçoit l'événement de création
    /// </summary>
    public async Task Handle(CreateCharacterEvent message)
    {
        // Enregistre les données initiales de la saga
        Data.CharacterId = message.CharacterId;
        Data.CreatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Saga démarrée pour le personnage {CharacterId}. En attente de confirmation...",
            message.CharacterId);

        // L'événement CreateCharacterEvent est déjà publié et sera reçu par l'autre microservice
        // La saga attend maintenant une réponse (Confirmed ou Failed)
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Étape 2a: L'autre microservice confirme - Tout est OK!
    /// </summary>
    public async Task Handle(CharacterCreationConfirmed message)
    {
        _logger.LogInformation(
            "Création du personnage {CharacterId} confirmée par l'autre service. Message: {Message}",
            message.CharacterId,
            message.Message);

        Data.IsCompleted = true;

        // La saga est terminée avec succès - on la marque comme complète
        MarkAsComplete();
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Étape 2b: L'autre microservice signale un échec - On compense!
    /// </summary>
    public async Task Handle(CharacterCreationFailed message)
    {
        _logger.LogWarning(
            "Création du personnage {CharacterId} échouée. Raison: {Reason}. Exécution de la compensation...",
            message.CharacterId,
            message.Reason);

        Data.IsFailed = true;
        Data.FailureReason = message.Reason;

        // COMPENSATION: On annule la création du personnage
        await CompensateCharacterCreation(message.CharacterId);

        // La saga est terminée (en échec)
        MarkAsComplete();
    }

    /// <summary>
    /// Compensation: Supprime le personnage créé localement
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
                _logger.LogInformation(
                    "Compensation exécutée: Personnage {CharacterId} supprimé",
                    characterId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Erreur lors de la compensation pour le personnage {CharacterId}", 
                characterId);
            // En production, vous voudriez peut-être republier un message pour retry
        }
    }
}

