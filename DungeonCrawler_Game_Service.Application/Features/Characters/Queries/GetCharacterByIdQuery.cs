using DefaultNamespace;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Queries;

/// <summary>
/// Query pour récupérer un personnage par son identifiant.
/// </summary>
public record GetCharacterByIdQuery(string Id) : IRequest<Character>;

