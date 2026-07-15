using FluentValidation.TestHelper;
using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Validators;
using Xunit;

namespace TicketManagementApi.Tests.Validators;

public class AddCommentRequestValidatorTests
{
    private readonly AddCommentRequestValidator _sut = new();

    [Fact]
    public void Validate_Given_EmptyMessage_Then_HasValidationErrorForMessage()
    {
        var request = new AddCommentRequest { AuthorRole = CommentAuthorRole.Client, Message = "" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Message);
    }

    [Fact]
    public void Validate_Given_UndefinedAuthorRoleValue_Then_HasValidationErrorForAuthorRole()
    {
        var request = new AddCommentRequest { AuthorRole = (CommentAuthorRole)999, Message = "Any update?" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.AuthorRole);
    }

    [Fact]
    public void Validate_Given_ValidRequest_Then_HasNoValidationError()
    {
        var request = new AddCommentRequest { AuthorRole = CommentAuthorRole.ITAgent, Message = "Looking into it" };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
