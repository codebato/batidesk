using NvnDesk.Application.DTOs.Tickets;

namespace NvnDesk.Application.Interfaces;

public interface ITicketService
{
    Task<TicketResponse> CreateAsync(CreateTicketRequest request, Guid currentUserId, Guid currentTenantId);
    Task<List<TicketResponse>> GetAllAsync(Guid currentTenantId);
    Task<TicketResponse?> GetByIdAsync(Guid ticketId, Guid currentTenantId);
    Task<TicketResponse> UpdateAsync(Guid ticketId, UpdateTicketRequest request, Guid currentTenantId);
}