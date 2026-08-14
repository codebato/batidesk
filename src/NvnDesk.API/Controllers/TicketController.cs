using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NvnDesk.Application.DTOs.Tickets;
using NvnDesk.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

namespace NvnDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IAIService _aiService;

    public TicketController(ITicketService ticketService, IAIService aiService)
    {
        _ticketService = ticketService;
        _aiService = aiService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub");
        return Guid.Parse(userIdClaim!);
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirstValue("tenantId");
        return Guid.Parse(tenantIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
    {
        try
        {
            var result = await _ticketService.CreateAsync(request, GetCurrentUserId(), GetCurrentTenantId());

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Geçersiz öncelik değeri." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ticketService.GetAllAsync(GetCurrentTenantId());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _ticketService.GetByIdAsync(id, GetCurrentTenantId());
        if (result is null)
        {
            return NotFound(new { message = "Ticket bulunamadı." });
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTicketRequest request)
    {
        try
        {
            var result = await _ticketService.UpdateAsync(id, request, GetCurrentTenantId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Geçersiz durum veya öncelik değeri." });
        }
    }
}


public class TestAIRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}