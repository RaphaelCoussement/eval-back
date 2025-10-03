using System.Net;
using DefaultNamespace;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Commands;

public class CreateCharacterCommand : IRequest<Character>
{
    public string Name { get; set; } = string.Empty;
    public int ClassCode { get; set; } 
    public string UserId { get; set; } = string.Empty;
    
}