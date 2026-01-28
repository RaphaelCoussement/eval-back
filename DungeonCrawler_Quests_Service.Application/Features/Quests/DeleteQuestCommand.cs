using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.Quests;

public record DeleteQuestCommand(Guid Id) : IRequest<bool>;