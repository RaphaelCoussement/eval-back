using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using FluentValidation;

namespace DungeonCrawler_Game_Service.Application.Features.Characters.Validators;

public class CreateCharacterCommandValidator : AbstractValidator<CreateCharacterCommand>
{
    /// <summary>
    /// Permet de valider les données d'une commande de création de personnage.
    /// </summary>
    public CreateCharacterCommandValidator()
    {
        // Règle de validation pour le nom du personnage.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");
        
        // Règle de validation pour le code de la classe du personnage.
        RuleFor(x => x.ClassCode)
            .InclusiveBetween(1, 3).WithMessage("ClassCode must be between 1 and 3 included.");
        
        // Règle de validation pour l'identifiant de l'utilisateur.
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
    
}