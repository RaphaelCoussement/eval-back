using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DefaultNamespace;

/// <summary>
/// Un personnage
/// </summary>
public class Character
{
    /// <summary>
    /// Identifiant du personnage
    /// </summary>
    [BsonId] // MongoDB Id
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// Nom du personnage
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Classe du personnage
    /// </summary>
    public Classes Class { get; set; }
    
    /// <summary>
    /// Identifiant Keycloak de l'utilisateur propriétaire du personnage (UUID Keycloak)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    
}