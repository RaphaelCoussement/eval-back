using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Queries;

/// <summary>
/// Query pour récupérer l'identifiant du skin équipé d'un personnage.
/// </summary>
public record GetEquippedSkinIdQuery(string CharacterId) : IRequest<string>;


