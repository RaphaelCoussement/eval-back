using DungeonCrawler_Quests_Service.Domain.Entities;
using DungeonCrawler_Quests_Service.Domain.Enums;
using DungeonCrawler_Quests_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.PlayerQuests;

public class PlayerQuestsHandlers : 
    IRequestHandler<GetPlayerQuestsQuery, List<PlayerQuest>>,
    IRequestHandler<ResetPlayerQuestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public PlayerQuestsHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<PlayerQuest>> Handle(GetPlayerQuestsQuery request, CancellationToken ct)
    {
        // 1. Récupérer les dépôts nécessaires
        var playerQuestRepo = _unitOfWork.GetRepository<PlayerQuest>();
        var questDefRepo = _unitOfWork.GetRepository<QuestDefinition>();

        // 2. Récupérer toutes les PlayerQuests du joueur
        var allPlayerQuests = await playerQuestRepo.GetAllAsync();
        var playerQuests = allPlayerQuests.Where(x => x.PlayerId == request.PlayerId).ToList();

        // 3. Récupérer le catalogue complet des quêtes
        var allQuestDefinitions = await questDefRepo.GetAllAsync();

        // 4. "Joindre" manuellement les données pour le Frontend
        foreach (var pq in playerQuests)
        {
            pq.Quest = allQuestDefinitions.FirstOrDefault(q => q.Id == pq.QuestId);
        }

        return playerQuests;
    }

    public async Task<bool> Handle(ResetPlayerQuestCommand request, CancellationToken ct)
    {
        var repo = _unitOfWork.GetRepository<PlayerQuest>();
        var all = await repo.GetAllAsync();
        var pq = all.FirstOrDefault(x => x.PlayerId == request.PlayerId && x.QuestId == request.QuestId);
        
        if (pq == null) return false;

        pq.ProgressCount = 0;
        pq.Status = QuestStatus.NotStarted;
        pq.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(pq);
        return true;
    }
}