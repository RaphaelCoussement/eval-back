namespace DungeonCrawler_Game_Service.Models.Requests;

/// <summary>
/// Modèle pour la création d'un personnage.
/// </summary>
public class CreateCharacterRequest
{
    /// <summary>
    /// Le nom du personnage.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// La classe du personnage.
    /// </summary>
    public int ClassCode { get; set; } 
    
    /// <summary>
    /// L'identifiant de l'utilisateur Keycloak (UUID Keycloak).
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
}