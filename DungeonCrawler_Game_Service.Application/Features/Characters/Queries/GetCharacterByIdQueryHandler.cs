using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Queries;

public class GetCharacterByIdQueryHandler(
    IUnitOfWork unitOfWork
    ) : IRequestHandler<GetCharacterByIdQuery, Character>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    /// <summary>
    /// Handler pour récupérer un personnage par son identifiant.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Le personnage correspondant à l'identifiant</returns>
    public async Task<Character> Handle(GetCharacterByIdQuery request, CancellationToken cancellationToken)
    {
        var character = await _characterRepository.GetByIdAsync(request.Id);
        
        if (character == null)
        {
            throw new KeyNotFoundException($"Character with id {request.Id} not found");
        }
        
        return character;
    }
}

