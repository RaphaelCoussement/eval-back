using DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;
using FluentValidation;
using MongoDB.Bson;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Validators;

public class LinkRoomsCommandValidator : AbstractValidator<LinkRoomsCommand>
{
    /// <summary>
    /// Règles de validation pour la requête GetNextRoomQuery.
    /// </summary>
    public LinkRoomsCommandValidator()
    {

        // Validation pour DungeonId (champ obligatoire et doit être un id MongoDB valide)
        RuleFor(x => x.DungeonId)
            .NotEmpty().WithMessage("DungeonId is required.")
            .Must(BeValidObjectId).WithMessage("DungeonId must be a valid MongoDB ObjectId.");
        
        // Validation pour ToRoomId (de forme "1a", "2b", "10c", "11b" etc., champ obligatoire)
        RuleFor(x => x.ToRoomId)
            .NotEmpty().WithMessage("ToRoomId is required.")
            .Matches(@"^\d+[a-zA-Z]$").WithMessage("ToRoomId must be in the format of a number followed by a letter (e.g., '1a', '2b').");
        
        // Validation pour FromRoomId (de forme "1a", "2b", "10c", "11b" etc., champ obligatoire)
        RuleFor(x => x.FromRoomId)
            .NotEmpty().WithMessage("ToRoomId is required.")
            .Matches(@"^\d+[a-zA-Z]$").WithMessage("FromRoomId must be in the format of a number followed by a letter (e.g., '1a', '2b').");
        
        // Validation afin que les numéros de salles se suivent (ex: 1a -> 2b, 2b -> 3a, 10a -> 11c etc.)
        RuleFor(x => x)
            .Must(command =>
            {
                var toRoomNumber = int.Parse(new string(command.ToRoomId.TakeWhile(char.IsDigit).ToArray()));
                var fromRoomNumber = int.Parse(new string(command.FromRoomId.TakeWhile(char.IsDigit).ToArray()));
                return toRoomNumber == fromRoomNumber + 1;
            })
            .WithMessage("ToRoomId must be the next sequential room number after FromRoomId (e.g., '1a' -> '2b', '2b' -> '3a').");
        
    }
    
    /// <summary>
    /// Permet de vérifier si une chaîne est un ObjectId MongoDB valide.
    /// </summary>
    private bool BeValidObjectId(string id)
    {
        return ObjectId.TryParse(id, out _);
    }
    
    
}