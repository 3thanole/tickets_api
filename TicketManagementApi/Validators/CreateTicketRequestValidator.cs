using FluentValidation;
using TicketManagementApi.DTOs;

namespace TicketManagementApi.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        // Règle technique : Priority doit être une valeur d'enum définie.
        // Le JsonStringEnumConverter actuel bloque les chaînes inconnues, mais
        // autorise par défaut un entier hors-limites (ex: 999) sans erreur.
        RuleFor(request => request.Priority)
            .IsInEnum();

        // Règle métier : une description est obligatoire pour créer un ticket
        // (changement volontaire : Description est optionnelle au niveau du DTO).
        RuleFor(request => request.Description)
            .NotEmpty()
            .WithMessage("Description is required.");
    }
}
