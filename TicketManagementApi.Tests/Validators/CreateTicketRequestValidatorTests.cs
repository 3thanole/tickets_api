using FluentValidation.TestHelper;
using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Validators;
using Xunit;

namespace TicketManagementApi.Tests.Validators;

public class CreateTicketRequestValidatorTests
{
    private readonly CreateTicketRequestValidator _sut = new();

    [Fact]
    public void Validate_Given_EmptyDescription_Then_HasValidationErrorForDescription()
    {
        var request = new CreateTicketRequest { Title = "Cannot log in", Description = "" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void Validate_Given_UndefinedPriorityValue_Then_HasValidationErrorForPriority()
    {
        var request = new CreateTicketRequest
        {
            Title = "Cannot log in",
            Description = "Details",
            Priority = (TicketPriority)999
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Priority);
    }

    [Fact]
    public void Validate_Given_ValidRequest_Then_HasNoValidationError()
    {
        var request = new CreateTicketRequest
        {
            Title = "Cannot log in",
            Description = "Details",
            Priority = TicketPriority.High
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
