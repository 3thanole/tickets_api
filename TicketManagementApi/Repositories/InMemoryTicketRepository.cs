using TicketManagementApi.Enums;
using TicketManagementApi.Models;

namespace TicketManagementApi.Repositories;

// Adapter sortant : implémente le port ITicketRepository. Contient tout ce qui
// était auparavant dans TicketService (liste, verrou, compteurs d'id) — ce sont
// des détails de stockage, pas des règles métier, donc ils vivent ici.
public class InMemoryTicketRepository : ITicketRepository
{
    private readonly object _lock = new();
    private readonly List<Ticket> _tickets = [];
    private int _nextId = 1;
    private int _nextCommentId = 1;

    public List<Ticket> GetAll()
    {
        lock (_lock)
        {
            return _tickets.Select(Clone).ToList();
        }
    }

    public Ticket? GetById(int id)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            return ticket is null ? null : Clone(ticket);
        }
    }

    public Ticket Add(Ticket ticket)
    {
        lock (_lock)
        {
            ticket.Id = _nextId++;
            _tickets.Add(ticket);
            return Clone(ticket);
        }
    }

    public Ticket? UpdateDetails(int id, string title, string? description, TicketPriority priority, DateTime updatedAt)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            if (ticket is null)
            {
                return null;
            }

            ticket.Title = title;
            ticket.Description = description;
            ticket.Priority = priority;
            ticket.UpdatedAt = updatedAt;

            return Clone(ticket);
        }
    }

    public Ticket? UpdateStatus(int id, TicketStatus status, DateTime updatedAt)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            if (ticket is null)
            {
                return null;
            }

            ticket.Status = status;
            ticket.UpdatedAt = updatedAt;

            return Clone(ticket);
        }
    }

    public bool Remove(int id)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            if (ticket is null)
            {
                return false;
            }

            _tickets.Remove(ticket);
            return true;
        }
    }

    public TicketComment? AddComment(int ticketId, TicketComment comment)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket is null)
            {
                return null;
            }

            comment.Id = _nextCommentId++;
            ticket.Comments.Add(comment);
            return comment;
        }
    }

    public void RemoveResolvedOlderThan(TimeSpan maxAge, DateTimeOffset now)
    {
        lock (_lock)
        {
            var nowUtc = now.UtcDateTime;

            _tickets.RemoveAll(ticket =>
                ticket is { Status: TicketStatus.Resolved, UpdatedAt: not null } &&
                (nowUtc - ticket.UpdatedAt.Value) >= maxAge);
        }
    }

    // Retourne une copie plutôt que l'objet interne : sans ça, l'appelant lirait
    // les champs du ticket après que ce verrou a déjà été relâché, ce qui pourrait
    // croiser une mutation faite par un autre thread pendant cette lecture.
    private static Ticket Clone(Ticket ticket) => new()
    {
        Id = ticket.Id,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status,
        Priority = ticket.Priority,
        CreatedAt = ticket.CreatedAt,
        UpdatedAt = ticket.UpdatedAt,
        Comments = ticket.Comments.Select(CloneComment).ToList()
    };

    private static TicketComment CloneComment(TicketComment comment) => new()
    {
        Id = comment.Id,
        TicketId = comment.TicketId,
        AuthorRole = comment.AuthorRole,
        Message = comment.Message,
        CreatedAt = comment.CreatedAt
    };
}
