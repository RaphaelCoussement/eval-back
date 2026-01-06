using DefaultNamespace;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Commands;

/// <summary>
///  Command pour équiper une skin à un personnage. Correspond a la requête reçue par le controller.
/// </summary>
public class EquipSkinCommand : IRequest<bool>
{
    public string CharacterId { get; set; } = string.Empty;
    public string SkinId { get; set; } = string.Empty;
}
