namespace NvnDesk.Application.DTOs.Tickets;


public class TicketResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Category { get; set; }
    public string? Summary { get; set; }
}