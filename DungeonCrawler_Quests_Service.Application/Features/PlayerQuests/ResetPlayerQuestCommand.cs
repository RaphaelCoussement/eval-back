using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.PlayerQuests;

public record ResetPlayerQuestCommand(Guid PlayerId, Guid QuestId) : IRequest<bool>;