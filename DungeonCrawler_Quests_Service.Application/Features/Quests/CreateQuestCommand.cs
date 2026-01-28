using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.Quests;

public record CreateQuestCommand(
    string Code, 
    string Title, 
    string Description, 
    int TargetCount, 
    string Reward) : IRequest<Guid>;