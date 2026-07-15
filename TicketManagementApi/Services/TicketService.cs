using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Models;
using TicketManagementApi.Repositories;

namespace TicketManagementApi.Services
{
    // Cœur métier : ne connaît que le port ITicketRepository, jamais comment/où
    // les tickets sont réellement stockés. Ne garde plus aucun état mutable.
    public class TicketService(ITicketRepository ticketRepository, TimeProvider timeProvider) : ITicketService
    {
        public TicketService(ITicketRepository ticketRepository) : this(ticketRepository, TimeProvider.System)
        {
        }

        public List<TicketResponse> GetAll() =>
            ticketRepository.GetAll().Select(MapToResponse).ToList();

        public TicketResponse? GetById(int id)
        {
            var ticket = ticketRepository.GetById(id);
            return ticket is null ? null : MapToResponse(ticket);
        }

        public TicketResponse Create(CreateTicketRequest request)
        {
            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
                UpdatedAt = null
            };

            var created = ticketRepository.Add(ticket);

            return MapToResponse(created);
        }

        public TicketResponse? Update(int id, UpdateTicketRequest request)
        {
            var updatedAt = timeProvider.GetUtcNow().UtcDateTime;
            var ticket = ticketRepository.UpdateDetails(id, request.Title, request.Description, request.Priority, updatedAt);

            return ticket is null ? null : MapToResponse(ticket);
        }

        public TicketResponse? UpdateStatus(int id, UpdateTicketStatusRequest request)
        {
            var updatedAt = timeProvider.GetUtcNow().UtcDateTime;
            var ticket = ticketRepository.UpdateStatus(id, request.Status, updatedAt);

            return ticket is null ? null : MapToResponse(ticket);
        }

        public bool Delete(int id) => ticketRepository.Remove(id);

        public TicketCommentResponse? AddComment(int ticketId, AddCommentRequest request)
        {
            var comment = new TicketComment
            {
                TicketId = ticketId,
                AuthorRole = request.AuthorRole,
                Message = request.Message,
                CreatedAt = timeProvider.GetUtcNow().UtcDateTime
            };

            var added = ticketRepository.AddComment(ticketId, comment);

            return added is null ? null : MapToCommentResponse(added);
        }

        public void CleanupResolvedTickets() =>
            ticketRepository.RemoveResolvedOlderThan(TimeSpan.FromMinutes(2), timeProvider.GetUtcNow());

        private static TicketResponse MapToResponse(Ticket ticket) => new()
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

        private static TicketCommentResponse MapToCommentResponse(TicketComment comment) => new()
        {
            Id = comment.Id,
            AuthorRole = comment.AuthorRole,
            Message = comment.Message,
            CreatedAt = comment.CreatedAt
        };
    }
}
