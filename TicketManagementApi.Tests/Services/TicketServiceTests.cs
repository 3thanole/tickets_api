using Shouldly;
using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Repositories;
using TicketManagementApi.Services;
using Xunit;

namespace TicketManagementApi.Tests.Services;

public class TicketServiceTests
{
    private readonly TicketService _sut = new(new InMemoryTicketRepository());

    [Fact]
    public void Create_Given_ValidRequest_Then_ReturnsTicketWithGeneratedIdAndOpenStatus()
    {
        var request = new CreateTicketRequest { Title = "Cannot log in", Priority = TicketPriority.High };

        var result = _sut.Create(request);

        result.Id.ShouldBe(1);
        result.Title.ShouldBe("Cannot log in");
        result.Priority.ShouldBe(TicketPriority.High);
        result.Status.ShouldBe(TicketStatus.Open);
        result.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_Given_MultipleRequests_Then_IncrementsId()
    {
        _sut.Create(new CreateTicketRequest { Title = "First" });
        var second = _sut.Create(new CreateTicketRequest { Title = "Second" });

        second.Id.ShouldBe(2);
    }

    [Fact]
    public void GetAll_Given_NoTickets_Then_ReturnsEmptyList()
    {
        _sut.GetAll().ShouldBeEmpty();
    }

    [Fact]
    public void GetAll_Given_ExistingTickets_Then_ReturnsAllOfThem()
    {
        _sut.Create(new CreateTicketRequest { Title = "First" });
        _sut.Create(new CreateTicketRequest { Title = "Second" });

        _sut.GetAll().Count.ShouldBe(2);
    }

    [Fact]
    public void GetById_Given_ExistingId_Then_ReturnsTicket()
    {
        var created = _sut.Create(new CreateTicketRequest { Title = "Cannot log in" });

        var result = _sut.GetById(created.Id);

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Cannot log in");
    }

    [Fact]
    public void GetById_Given_UnknownId_Then_ReturnsNull()
    {
        _sut.GetById(999).ShouldBeNull();
    }

    [Fact]
    public void Update_Given_ExistingId_Then_UpdatesFieldsAndSetsUpdatedAt()
    {
        var created = _sut.Create(new CreateTicketRequest { Title = "Original title" });
        var request = new UpdateTicketRequest { Title = "New title", Priority = TicketPriority.Urgent };

        var result = _sut.Update(created.Id, request);

        result.ShouldNotBeNull();
        result.Title.ShouldBe("New title");
        result.Priority.ShouldBe(TicketPriority.Urgent);
        result.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Update_Given_UnknownId_Then_ReturnsNull()
    {
        var request = new UpdateTicketRequest { Title = "New title" };

        _sut.Update(999, request).ShouldBeNull();
    }

    [Fact]
    public void UpdateStatus_Given_ExistingId_Then_UpdatesStatusAndSetsUpdatedAt()
    {
        var created = _sut.Create(new CreateTicketRequest { Title = "Cannot log in" });
        var request = new UpdateTicketStatusRequest { Status = TicketStatus.Resolved };

        var result = _sut.UpdateStatus(created.Id, request);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(TicketStatus.Resolved);
        result.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateStatus_Given_UnknownId_Then_ReturnsNull()
    {
        var request = new UpdateTicketStatusRequest { Status = TicketStatus.Resolved };

        _sut.UpdateStatus(999, request).ShouldBeNull();
    }

    [Fact]
    public void Delete_Given_ExistingId_Then_RemovesTicketAndReturnsTrue()
    {
        var created = _sut.Create(new CreateTicketRequest { Title = "Cannot log in" });

        var deleted = _sut.Delete(created.Id);

        deleted.ShouldBeTrue();
        _sut.GetById(created.Id).ShouldBeNull();
    }

    [Fact]
    public void Delete_Given_UnknownId_Then_ReturnsFalse()
    {
        _sut.Delete(999).ShouldBeFalse();
    }

    [Fact]
    public void AddComment_Given_ExistingTicket_Then_AddsCommentAndReturnsIt()
    {
        var created = _sut.Create(new CreateTicketRequest { Title = "Cannot log in" });
        var request = new AddCommentRequest { AuthorRole = CommentAuthorRole.Client, Message = "Any update?" };

        var comment = _sut.AddComment(created.Id, request);

        comment.ShouldNotBeNull();
        comment.AuthorRole.ShouldBe(CommentAuthorRole.Client);
        comment.Message.ShouldBe("Any update?");

        _sut.GetById(created.Id)!.Comments.ShouldHaveSingleItem();
    }

    [Fact]
    public void AddComment_Given_UnknownTicket_Then_ReturnsNull()
    {
        var request = new AddCommentRequest { AuthorRole = CommentAuthorRole.ITAgent, Message = "Looking into it" };

        _sut.AddComment(999, request).ShouldBeNull();
    }

    [Fact]
    public void CleanupResolvedTickets_Given_ResolvedTicketOlderThan2Minutes_Then_DeletesIt()
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var sut = new TicketService(new InMemoryTicketRepository(), timeProvider);
        var created = sut.Create(new CreateTicketRequest { Title = "Cannot log in" });
        sut.UpdateStatus(created.Id, new UpdateTicketStatusRequest { Status = TicketStatus.Resolved });

        timeProvider.Advance(TimeSpan.FromMinutes(6));
        sut.CleanupResolvedTickets();

        sut.GetById(created.Id).ShouldBeNull();
    }

    [Fact]
    public void CleanupResolvedTickets_Given_ResolvedTicketWithinLast2Minutes_Then_KeepsIt()
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var sut = new TicketService(new InMemoryTicketRepository(), timeProvider);
        var created = sut.Create(new CreateTicketRequest { Title = "Cannot log in" });
        sut.UpdateStatus(created.Id, new UpdateTicketStatusRequest { Status = TicketStatus.Resolved });

        timeProvider.Advance(TimeSpan.FromMinutes(1));
        sut.CleanupResolvedTickets();

        sut.GetById(created.Id).ShouldNotBeNull();
    }

    [Fact]
    public void CleanupResolvedTickets_Given_NonResolvedTicketRegardlessOfAge_Then_KeepsIt()
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var sut = new TicketService(new InMemoryTicketRepository(), timeProvider);
        var created = sut.Create(new CreateTicketRequest { Title = "Cannot log in" });

        timeProvider.Advance(TimeSpan.FromMinutes(10));
        sut.CleanupResolvedTickets();

        sut.GetById(created.Id).ShouldNotBeNull();
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan by) => _utcNow += by;
    }
}
