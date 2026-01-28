using DungeonCrawler_Quests_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.PlayerQuests;

public record GetPlayerQuestsQuery(Guid PlayerId) : IRequest<List<PlayerQuest>>;