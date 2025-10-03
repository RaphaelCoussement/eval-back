using DungeonCrawler_Game_Service.Application.Features.Dungeons.Queries;
using FluentValidation;
using MongoDB.Bson;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Validators;

public class GetNextRoomQueryValidator : AbstractValidator<GetNextRoomsQuery>
{
    /// <summary>
    /// Règles de validation pour la requête GetNextRoomQuery.
    /// </summary>
    public GetNextRoomQueryValidator()
    {
        // Validation pour CurrentRoomId (champ obligatoire et doit être un id MongoDB valide)
        RuleFor(x => x.CurrentRoomId)
            .NotEmpty().WithMessage("CurrentRoomId is required.")
            .Matches(@"^\d+[a-zA-Z]$").WithMessage("CurrentRoomId must be in the format of a number followed by a letter (e.g., '1a', '2b').");

        // Validation pour DungeonId (champ obligatoire et doit être un id MongoDB valide)
        RuleFor(x => x.DungeonId)
            .NotEmpty().WithMessage("DungeonId is required.")
            .Must(BeValidObjectId).WithMessage("DungeonId must be a valid MongoDB ObjectId.");
    }
    
    private bool BeValidObjectId(string id)
    {
        return ObjectId.TryParse(id, out _);
    }
    
}