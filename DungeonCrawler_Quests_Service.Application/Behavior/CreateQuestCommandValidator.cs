using DungeonCrawler_Quests_Service.Application.Features.Quests;
using DungeonCrawler_Quests_Service.Domain.Entities;
using DungeonCrawler_Quests_Service.Infrastructure.Interfaces; // Vérifie le namespace
using FluentValidation;

namespace DungeonCrawler_Quests_Service.Application.Behavior;

public class CreateQuestCommandValidator : AbstractValidator<CreateQuestCommand>
{
    // On injecte le repository spécifique à l'entité Quest
    public CreateQuestCommandValidator(IRepository<QuestDefinition> repository) 
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code est obligatoire.")
            .MustAsync(async (code, cancellation) => 
            {
                // Vérifie si un document avec ce code existe déjà
                bool exists = await repository.ExistsAsync(q => q.Code == code);
                return !exists;
            })
            .WithMessage("Ce code unique est déjà utilisé par une autre quête.");
    }
}