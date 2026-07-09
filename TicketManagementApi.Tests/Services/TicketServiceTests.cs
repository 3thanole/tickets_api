using Shouldly;
using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Services;
using Xunit;

namespace TicketManagementApi.Tests.Services;

public class TicketServiceTests
{
    private readonly TicketService _sut = new();

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
}
