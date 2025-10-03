namespace DungeonCrawler_Game_Service.Domain.Entities;

/// <summary>
/// Représente un lien entre deux salles dans un donjon.
/// </summary>
public class RoomLink
{
    /// <summary>
    /// Id de la room d'où provient le lien.
    /// </summary>
    public string FromRoomId { get; set; } = default!;
    /// <summary>
    /// Id de la room vers laquelle pointe le lien.
    /// </summary>
    public string ToRoomId { get; set; } = default!;
}