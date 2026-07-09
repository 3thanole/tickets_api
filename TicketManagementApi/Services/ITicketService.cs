using TicketManagementApi.DTOs;

namespace TicketManagementApi.Services
{
    public interface ITicketService
    {
        List<TicketResponse> GetAll();
        TicketResponse? GetById(int id);
        TicketResponse Create(CreateTicketRequest request);
        TicketResponse? Update(int id, UpdateTicketRequest request);
        TicketResponse? UpdateStatus(int id, UpdateTicketStatusRequest request);
        bool Delete(int id);
    }
}