using DungeonCrawler_Quests_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.Quests;

public record GetQuestByIdQuery(Guid Id) : IRequest<QuestDefinition?>;