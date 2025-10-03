using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Commands;

public class CreateCharacterCommandHandler(
    IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateCharacterCommand, Character>
{
    private readonly IRepository<Character> _characterRepository = unitOfWork.GetRepository<Character>();
    
    public async Task<Character> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        var character = new Character{ Name = request.Name, Class = (Classes)request.ClassCode, UserId = request.UserId };

        await _characterRepository.AddAsync(character);
        return character;
    }
}