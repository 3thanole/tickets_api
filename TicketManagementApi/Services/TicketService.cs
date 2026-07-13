using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Models;

namespace TicketManagementApi.Services
{
    public class TicketService(TimeProvider timeProvider) : ITicketService
    {
        private readonly object _lock = new();
        private readonly List<Ticket> _tickets = [];
        private int _nextId = 1;
        private int _nextCommentId = 1;

        public TicketService() : this(TimeProvider.System)
        {
        }

        public List<TicketResponse> GetAll()
        {
            lock (_lock)
            {
                return _tickets
                    .Select(MapToResponse)
                    .ToList();
            }
        }

        public TicketResponse? GetById(int id)
        {
            lock (_lock)
            {
                var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

                if (ticket is null)
                {
                    return null;
                }

                return MapToResponse(ticket);
            }
        }

        public TicketResponse Create(CreateTicketRequest request)
        {
            lock (_lock)
            {
                var ticket = new Ticket
                {
                    Id = _nextId,
                    Title = request.Title,
                    Description = request.Description,
                    Priority = request.Priority,
                    Status = TicketStatus.Open,
                    CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
                    UpdatedAt = null
                };
    
                _tickets.Add(ticket);
                _nextId++;

                return MapToResponse(ticket);
            }
        }

        public TicketResponse? Update(int id, UpdateTicketRequest request)
        {
            lock (_lock)
            {
                var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

                if (ticket is null)
                {
                    return null;
                }

                ticket.Title = request.Title;
                ticket.Description = request.Description;
                ticket.Priority = request.Priority;
                ticket.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

                return MapToResponse(ticket);
            }
        }

        public TicketResponse? UpdateStatus(int id, UpdateTicketStatusRequest request)
        {
            lock (_lock)
            {
                var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

                if (ticket is null)
                {
                    return null;
                }

                ticket.Status = request.Status;
                ticket.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

                return MapToResponse(ticket);
            }
        }

        public bool Delete(int id)
        {
            lock (_lock)
            {
                var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

                if (ticket is null)
                {
                    return false;
                }

                _tickets.Remove(ticket);

                return true;
            }
        }

        public TicketCommentResponse? AddComment(int ticketId, AddCommentRequest request)
        {
            lock (_lock)
            {
                var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == ticketId);

                if (ticket is null)
                {
                    return null;
                }

                var comment = new TicketComment
                {
                    Id = _nextCommentId,
                    TicketId = ticketId,
                    AuthorRole = request.AuthorRole,
                    Message = request.Message,
                    CreatedAt = timeProvider.GetUtcNow().UtcDateTime
                };

                ticket.Comments.Add(comment);
                _nextCommentId++;

                return MapToCommentResponse(comment);
            }
        }

        public void CleanupResolvedTickets()
        {
            lock (_lock)
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;

                _tickets.RemoveAll(ticket =>
                    ticket is { Status: TicketStatus.Resolved, UpdatedAt: not null } &&
                    (now - ticket.UpdatedAt.Value) >= TimeSpan.FromMinutes(2));
            }
        }

        private static TicketResponse MapToResponse(Ticket ticket)
        {
            return new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Comments = ticket.Comments.Select(MapToCommentResponse).ToList()
            };
        }

        private static TicketCommentResponse MapToCommentResponse(TicketComment comment)
        {
            return new TicketCommentResponse
            {
                Id = comment.Id,
                AuthorRole = comment.AuthorRole,
                Message = comment.Message,
                CreatedAt = comment.CreatedAt
            };
        }
    }
}
