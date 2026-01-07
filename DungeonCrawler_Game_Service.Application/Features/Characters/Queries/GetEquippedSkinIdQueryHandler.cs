using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Queries;

public class GetEquippedSkinIdQueryHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<GetEquippedSkinIdQuery, string>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    /// <summary>
    /// Handler pour récupérer l'identifiant du skin équipé d'un personnage.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>L'identifiant du skin équipé</returns>
    public async Task<string> Handle(GetEquippedSkinIdQuery request, CancellationToken cancellationToken)
    {
        var character = await _characterRepository.GetByIdAsync(request.CharacterId);
        
        if (character == null)
        {
            throw new KeyNotFoundException($"Character with id {request.CharacterId} not found");
        }
        
        return character.ActiveSkinId;
    }
}

