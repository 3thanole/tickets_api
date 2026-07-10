using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using TicketManagementApi.Controllers;
using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Services;
using Xunit;

namespace TicketManagementApi.Tests.Controllers;

public class TicketsControllerTests
{
    private readonly Mock<ITicketService> _ticketServiceMock = new();
    private readonly TicketsController _sut;

    public TicketsControllerTests()
    {
        _sut = new TicketsController(_ticketServiceMock.Object);
    }

    [Fact]
    public void GetAll_Then_ReturnsOkWithTickets()
    {
        var tickets = new List<TicketResponse> { new() { Id = 1, Title = "Cannot log in" } };
        _ticketServiceMock.Setup(s => s.GetAll()).Returns(tickets);

        var result = _sut.GetAll();

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(tickets);
    }

    [Fact]
    public void GetById_Given_ExistingId_Then_ReturnsOkWithTicket()
    {
        var ticket = new TicketResponse { Id = 1, Title = "Cannot log in" };
        _ticketServiceMock.Setup(s => s.GetById(1)).Returns(ticket);

        var result = _sut.GetById(1);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(ticket);
    }

    [Fact]
    public void GetById_Given_UnknownId_Then_ReturnsNotFound()
    {
        _ticketServiceMock.Setup(s => s.GetById(999)).Returns((TicketResponse?)null);

        var result = _sut.GetById(999);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public void Create_Then_ReturnsCreatedAtActionWithTicket()
    {
        var request = new CreateTicketRequest { Title = "Cannot log in" };
        var created = new TicketResponse { Id = 1, Title = "Cannot log in" };
        _ticketServiceMock.Setup(s => s.Create(request)).Returns(created);

        var result = _sut.Create(request);

        var createdResult = result.Result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(TicketsController.GetById));
        createdResult.RouteValues!["id"].ShouldBe(1);
        createdResult.Value.ShouldBe(created);
    }

    [Fact]
    public void Update_Given_ExistingId_Then_ReturnsOkWithTicket()
    {
        var request = new UpdateTicketRequest { Title = "New title" };
        var updated = new TicketResponse { Id = 1, Title = "New title" };
        _ticketServiceMock.Setup(s => s.Update(1, request)).Returns(updated);

        var result = _sut.Update(1, request);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(updated);
    }

    [Fact]
    public void Update_Given_UnknownId_Then_ReturnsNotFound()
    {
        var request = new UpdateTicketRequest { Title = "New title" };
        _ticketServiceMock.Setup(s => s.Update(999, request)).Returns((TicketResponse?)null);

        var result = _sut.Update(999, request);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public void UpdateStatus_Given_ExistingId_Then_ReturnsOkWithTicket()
    {
        var request = new UpdateTicketStatusRequest { Status = TicketStatus.Resolved };
        var updated = new TicketResponse { Id = 1, Status = TicketStatus.Resolved };
        _ticketServiceMock.Setup(s => s.UpdateStatus(1, request)).Returns(updated);

        var result = _sut.UpdateStatus(1, request);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(updated);
    }

    [Fact]
    public void UpdateStatus_Given_UnknownId_Then_ReturnsNotFound()
    {
        var request = new UpdateTicketStatusRequest { Status = TicketStatus.Resolved };
        _ticketServiceMock.Setup(s => s.UpdateStatus(999, request)).Returns((TicketResponse?)null);

        var result = _sut.UpdateStatus(999, request);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public void Delete_Given_ExistingId_Then_ReturnsNoContent()
    {
        _ticketServiceMock.Setup(s => s.Delete(1)).Returns(true);

        var result = _sut.Delete(1);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void Delete_Given_UnknownId_Then_ReturnsNotFound()
    {
        _ticketServiceMock.Setup(s => s.Delete(999)).Returns(false);

        var result = _sut.Delete(999);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public void AddComment_Given_ExistingTicket_Then_ReturnsCreatedAtActionWithComment()
    {
        var request = new AddCommentRequest { AuthorRole = CommentAuthorRole.ITAgent, Message = "On it" };
        var comment = new TicketCommentResponse { Id = 1, AuthorRole = CommentAuthorRole.ITAgent, Message = "On it" };
        _ticketServiceMock.Setup(s => s.AddComment(1, request)).Returns(comment);

        var result = _sut.AddComment(1, request);

        var createdResult = result.Result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(TicketsController.GetById));
        createdResult.RouteValues!["id"].ShouldBe(1);
        createdResult.Value.ShouldBe(comment);
    }

    [Fact]
    public void AddComment_Given_UnknownTicket_Then_ReturnsNotFound()
    {
        var request = new AddCommentRequest { AuthorRole = CommentAuthorRole.Client, Message = "Any update?" };
        _ticketServiceMock.Setup(s => s.AddComment(999, request)).Returns((TicketCommentResponse?)null);

        var result = _sut.AddComment(999, request);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }
}
