using DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;
using FluentValidation;
using MongoDB.Bson;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Validators;

public class EnterRoomQueryValidator : AbstractValidator<EnterRoomQuery>
{
    /// <summary>
    /// Gére la validation des requêtes pour entrer dans une salle spécifique d'un donjon.
    /// </summary>
    public EnterRoomQueryValidator()
    {
        // Validation de NextRoomId (champ obligatoire et id MongoDB valide)
        RuleFor(x => x.NextRoomId)
            .NotEmpty().WithMessage("NextRoomId is required.")
            .Must(BeValidObjectId).WithMessage("NextRoomId must be a valid MongoDB ObjectId.");

        // Validation de DungeonId (champ obligatoire et id MongoDB valide)
        RuleFor(x => x.DungeonId)
            .NotEmpty().WithMessage("DungeonId is required.")
            .Must(BeValidObjectId).WithMessage("DungeonId must be a valid MongoDB ObjectId.");
    }

    /// <summary>
    /// Vérifie si une chaîne est un ObjectId MongoDB valide.
    /// </summary>
    /// <param name="id">Id testé</param>
    private bool BeValidObjectId(string id)
    {
        return ObjectId.TryParse(id, out _);
    }
}