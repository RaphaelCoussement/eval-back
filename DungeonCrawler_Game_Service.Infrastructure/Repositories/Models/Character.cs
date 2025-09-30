namespace DefaultNamespace;

/// <summary>
/// Un personnage
/// </summary>
public class Character
{
    /// <summary>
    /// Identifiant du personnage
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nom du personnage
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Classe du personnage
    /// </summary>
    public Classes Class { get; set; }
}