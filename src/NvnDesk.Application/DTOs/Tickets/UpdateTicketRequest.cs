namespace NvnDesk.Application.DTOs.Tickets;

public class UpdateTicketRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
}