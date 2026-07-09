using TicketManagementApi.DTOs;
using TicketManagementApi.Enums;
using TicketManagementApi.Models;

namespace TicketManagementApi.Services
{
    public class TicketService : ITicketService
    {
        private readonly List<Ticket> _tickets = new();
        private int _nextId = 1;

        public List<TicketResponse> GetAll()
        {
            return _tickets
                .Select(MapToResponse)
                .ToList();
        }

        public TicketResponse? GetById(int id)
        {
            var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

            if (ticket is null)
            {
                return null;
            }

            return MapToResponse(ticket);
        }

        public TicketResponse Create(CreateTicketRequest request)
        {
            var ticket = new Ticket
            {
                Id = _nextId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _tickets.Add(ticket);
            _nextId++;

            return MapToResponse(ticket);
        }

        public TicketResponse? Update(int id, UpdateTicketRequest request)
        {
            var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

            if (ticket is null)
            {
                return null;
            }

            ticket.Title = request.Title;
            ticket.Description = request.Description;
            ticket.Priority = request.Priority;
            ticket.UpdatedAt = DateTime.UtcNow;

            return MapToResponse(ticket);
        }

        public TicketResponse? UpdateStatus(int id, UpdateTicketStatusRequest request)
        {
            var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

            if (ticket is null)
            {
                return null;
            }

            ticket.Status = request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            return MapToResponse(ticket);
        }

        public bool Delete(int id)
        {
            var ticket = _tickets.FirstOrDefault(ticket => ticket.Id == id);

            if (ticket is null)
            {
                return false;
            }

            _tickets.Remove(ticket);

            return true;
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
                UpdatedAt = ticket.UpdatedAt
            };
        }
    }
}