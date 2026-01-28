using DungeonCrawler_Quests_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.Quests;

public record GetAllQuestsQuery(bool? IsActiveOnly = null, string? SearchKeyword = null) : IRequest<List<QuestDefinition>>;