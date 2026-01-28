using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.Quests;

public record UpdateQuestCommand(Guid Id, string Title, string Description, int TargetCount, string Reward, bool IsActive) : IRequest<bool>;