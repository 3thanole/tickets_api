using FluentValidation;
using TicketManagementApi.DTOs;

namespace TicketManagementApi.Validators;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        // Règle technique : AuthorRole doit être une valeur d'enum définie.
        RuleFor(request => request.AuthorRole)
            .IsInEnum();

        // Règle métier : un message vide n'a pas de sens.
        RuleFor(request => request.Message)
            .NotEmpty()
            .WithMessage("Message must not be empty.");
    }
}
