using DungeonCrawler_Quests_Service.Domain.Entities;
using DungeonCrawler_Quests_Service.Domain.Enums;
using DungeonCrawler_Quests_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly.Messages;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace DungeonCrawler_Quests_Service.Application.Features.Events;

public class DungeonCompletedHandler : IHandleMessages<DungeonCompleted>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DungeonCompletedHandler> _logger;

    public DungeonCompletedHandler(IUnitOfWork unitOfWork, ILogger<DungeonCompletedHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DungeonCompleted message)
    {
        _logger.LogInformation("Processing DungeonCompleted event: {EventId} for Player: {PlayerId}", message.EventId, message.PlayerId);

        // 1. IDEMPOTENCE : Vérifier si l'événement a déjà été traité
        var eventRepo = _unitOfWork.GetRepository<ProcessedEvent>();
        var alreadyProcessed = (await eventRepo.GetAllAsync()).Any(x => x.EventId == message.EventId);

        if (alreadyProcessed)
        {
            _logger.LogWarning("Event {EventId} already processed. Skipping.", message.EventId);
            return;
        }

        // 2. RÉCUPÉRATION DES QUÊTES : On cherche les quêtes de type "DUNGEON_COMPLETION" qui sont actives
        var questRepo = _unitOfWork.GetRepository<QuestDefinition>();
        var activeQuests = (await questRepo.GetAllAsync())
            .Where(q => q.IsActive && q.Type == "DUNGEON_COMPLETION")
            .ToList();

        if (!activeQuests.Any()) return;

        // 3. MISE À JOUR DE LA PROGRESSION
        var playerQuestRepo = _unitOfWork.GetRepository<PlayerQuest>();
        var playerQuests = (await playerQuestRepo.GetAllAsync())
            .Where(pq => pq.PlayerId == message.PlayerId)
            .ToList();

        foreach (var quest in activeQuests)
        {
            var pq = playerQuests.FirstOrDefault(x => x.QuestId == quest.Id);

            // Si le joueur n'a pas encore cette quête, on lui crée son suivi
            if (pq == null)
            {
                pq = new PlayerQuest
                {
                    Id = Guid.NewGuid(),
                    PlayerId = message.PlayerId,
                    QuestId = quest.Id,
                    Status = QuestStatus.NotStarted,
                    ProgressCount = 0
                };
                await playerQuestRepo.AddAsync(pq);
            }

            // Utilisation de la logique du Domain (méthode qu'on a créée dans l'entité)
            pq.UpdateProgress(1, quest.TargetCount);
            await playerQuestRepo.UpdateAsync(pq);
            
            _logger.LogInformation("Updated progress for Quest {QuestCode}: {Progress}/{Target}", quest.Code, pq.ProgressCount, quest.TargetCount);
        }

        // 4. MARQUER COMME TRAITÉ : On enregistre l'EventId pour ne plus le traiter
        await eventRepo.AddAsync(new ProcessedEvent { EventId = message.EventId });
    }
}