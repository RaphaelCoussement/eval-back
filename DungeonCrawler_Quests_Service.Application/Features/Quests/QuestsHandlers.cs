using DungeonCrawler_Quests_Service.Domain.Entities;
using DungeonCrawler_Quests_Service.Infrastructure.Interfaces;
using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Features.Quests;

public class QuestsHandlers : 
    IRequestHandler<GetAllQuestsQuery, List<QuestDefinition>>,
    IRequestHandler<GetQuestByIdQuery, QuestDefinition?>,
    IRequestHandler<CreateQuestCommand, Guid>, // Ajouté ici
    IRequestHandler<UpdateQuestCommand, bool>,
    IRequestHandler<DeleteQuestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public QuestsHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    // GET ALL
    public async Task<List<QuestDefinition>> Handle(GetAllQuestsQuery request, CancellationToken ct)
    {
        var repo = _unitOfWork.GetRepository<QuestDefinition>();
        var quests = await repo.GetAllAsync();
        
        var query = quests.AsQueryable();
        if (request.IsActiveOnly.HasValue) query = query.Where(x => x.IsActive == request.IsActiveOnly.Value);
        if (!string.IsNullOrEmpty(request.SearchKeyword)) 
            query = query.Where(x => x.Title.Contains(request.SearchKeyword, StringComparison.OrdinalIgnoreCase) || 
                                     x.Code.Contains(request.SearchKeyword, StringComparison.OrdinalIgnoreCase));
        
        return query.ToList();
    }

    // GET BY ID
    public async Task<QuestDefinition?> Handle(GetQuestByIdQuery request, CancellationToken ct) 
        => await _unitOfWork.GetRepository<QuestDefinition>().GetByIdAsync(request.Id);

    // POST (CREATE)
    public async Task<Guid> Handle(CreateQuestCommand request, CancellationToken ct)
    {
        var repo = _unitOfWork.GetRepository<QuestDefinition>();
        
        // Règle métier : Code unique
        var allQuests = await repo.GetAllAsync();
        if (allQuests.Any(q => q.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"Le code quête '{request.Code}' est déjà utilisé.");
        }

        var newQuest = new QuestDefinition
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            TargetCount = request.TargetCount,
            Reward = request.Reward,
            IsActive = true // Active par défaut
        };

        await repo.AddAsync(newQuest);
        return newQuest.Id;
    }

    // PUT (UPDATE)
    public async Task<bool> Handle(UpdateQuestCommand request, CancellationToken ct)
    {
        var repo = _unitOfWork.GetRepository<QuestDefinition>();
        var quest = await repo.GetByIdAsync(request.Id);
        if (quest == null) return false;

        quest.Title = request.Title;
        quest.Description = request.Description;
        quest.TargetCount = request.TargetCount;
        quest.Reward = request.Reward;
        quest.IsActive = request.IsActive;

        await repo.UpdateAsync(quest);
        return true;
    }

    // DELETE
    public async Task<bool> Handle(DeleteQuestCommand request, CancellationToken ct)
    {
        var repo = _unitOfWork.GetRepository<QuestDefinition>();
        var quest = await repo.GetByIdAsync(request.Id);
        if (quest == null) return false;

        quest.IsActive = false; // Soft delete
        await repo.UpdateAsync(quest);
        return true;
    }
}