using Shouldly;
using TicketManagementApi.Enums;
using TicketManagementApi.Models;
using TicketManagementApi.Repositories;
using Xunit;

namespace TicketManagementApi.Tests.Repositories;

public class InMemoryTicketRepositoryTests
{
    private readonly InMemoryTicketRepository _sut = new();

    [Fact]
    public void Add_Given_MultipleTickets_Then_AssignsSequentialIds()
    {
        var first = _sut.Add(new Ticket { Title = "First" });
        var second = _sut.Add(new Ticket { Title = "Second" });

        first.Id.ShouldBe(1);
        second.Id.ShouldBe(2);
    }

    [Fact]
    public void AddComment_Given_MultipleComments_Then_AssignsSequentialCommentIds()
    {
        var ticket = _sut.Add(new Ticket { Title = "Cannot log in" });

        var firstComment = _sut.AddComment(ticket.Id, new TicketComment { Message = "First" });
        var secondComment = _sut.AddComment(ticket.Id, new TicketComment { Message = "Second" });

        firstComment!.Id.ShouldBe(1);
        secondComment!.Id.ShouldBe(2);
    }

    [Fact]
    public void RemoveResolvedOlderThan_Given_TicketExactlyAtThreshold_Then_RemovesIt()
    {
        var now = DateTimeOffset.UtcNow;
        var ticket = _sut.Add(new Ticket { Title = "Cannot log in" });
        _sut.UpdateStatus(ticket.Id, TicketStatus.Resolved, now.UtcDateTime - TimeSpan.FromMinutes(2));

        _sut.RemoveResolvedOlderThan(TimeSpan.FromMinutes(2), now);

        _sut.GetById(ticket.Id).ShouldBeNull();
    }

    [Fact]
    public void GetById_Given_MutatingReturnedTicket_Then_DoesNotAffectStoredTicket()
    {
        var created = _sut.Add(new Ticket { Title = "Original title" });

        var fetched = _sut.GetById(created.Id)!;
        fetched.Title = "Mutated locally, should not stick";

        _sut.GetById(created.Id)!.Title.ShouldBe("Original title");
    }
}
