using Rebus.Sagas;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Sagas;

/// <summary>
/// Données de la saga - Stocke l'état pendant le processus de création
/// </summary>
public class CreateCharacterSagaData : ISagaData
{
    // Propriétés requises par Rebus
    public Guid Id { get; set; }
    public int Revision { get; set; }
    
    // Données métier
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    
    // État de la saga
    public bool IsCompleted { get; set; }
    public bool IsFailed { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

