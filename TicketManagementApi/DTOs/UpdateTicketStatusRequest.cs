using TicketManagementApi.Enums;

namespace TicketManagementApi.DTOs
{
    public class UpdateTicketStatusRequest
    {
        public TicketStatus Status { get; set; }
    }
}