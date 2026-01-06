using DefaultNamespace;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;
using Rebus.Bus;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Commands;

/// <summary>
///  Handler pour équiper une skin à un personnage
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="bus"></param>
public class EquipSkinCommandHandler(
    IUnitOfWork unitOfWork,
    IBus bus
) : IRequestHandler<EquipSkinCommand, bool>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();

    /// <summary>
    /// Handler pour équiper une skin à un personnage
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> Handle(EquipSkinCommand request, CancellationToken cancellationToken)
    {
        // Récupère le personnage via son Id
        var character = await _characterRepository.GetByIdAsync(request.CharacterId);
        if (character == null)
        {
            throw new Exception("Character not found");
        }

        // Met à jour la skin active du personnage
        character.ActiveSkinId = request.SkinId;
        await _characterRepository.UpdateAsync(character);
        
        return true;
    }
}
