using TicketManagementApi.Enums;
using TicketManagementApi.Models;

namespace TicketManagementApi.Repositories;

public interface ITicketRepository
{
    List<Ticket> GetAll();
    Ticket? GetById(int id);
    Ticket Add(Ticket ticket);
    Ticket? UpdateDetails(int id, string title, string? description, TicketPriority priority, DateTime updatedAt);
    Ticket? UpdateStatus(int id, TicketStatus status, DateTime updatedAt);
    bool Remove(int id);
    TicketComment? AddComment(int ticketId, TicketComment comment);
    void RemoveResolvedOlderThan(TimeSpan maxAge, DateTimeOffset now);
}
