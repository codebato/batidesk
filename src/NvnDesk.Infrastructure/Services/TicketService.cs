using Microsoft.EntityFrameworkCore;
using NvnDesk.Application.DTOs.Tickets;
using NvnDesk.Application.Interfaces;
using NvnDesk.Domain.Entities;
using NvnDesk.Infrastructure.Persistence;
using StackExchange.Redis;
using System.Text.Json;
using NvnDesk.Application.Services;
using Microsoft.AspNetCore.SignalR;
using NvnDesk.Infrastructure.Hubs;
using Hangfire;

namespace NvnDesk.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly NvnDeskDbContext _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly IEmailService _emailService;
    private readonly IAIService _aiService;
    private readonly IHubContext<TicketHub> _hubContext;

    public TicketService(NvnDeskDbContext context, IConnectionMultiplexer redis, IEmailService emailService, IAIService aiService, IHubContext<TicketHub> hubContext)
    {
        _context = context;
        _redis = redis;
        _emailService = emailService;
        _aiService = aiService;
        _hubContext = hubContext;
    }

    // Cache key'ini tek bir yerden üretmek için küçük bir yardımcı metot.
    // Aynı formatı 3 farklı yerde (GetAll, Create, Update) elle yazmak yerine
    // buradan çağırıyoruz — ileride key formatı değişirse tek yerden değişir.
    private static string GetCacheKey(Guid tenantId) => $"tickets:tenant:{tenantId}";

    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, Guid currentUserId, Guid currentTenantId)
    {
        var priority = Enum.Parse<TicketPriority>(request.Priority, ignoreCase: true);

        var ticket = new Ticket
        {
            TenantId = currentTenantId,
            Title = request.Title,
            Description = request.Description,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedByUserId = currentUserId
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // ÖNEMLİ: Yeni ticket veritabanına yazıldı, ama Redis'teki liste cache'i
        // hâlâ eski (bu ticket'ı içermeyen) halini tutuyor. Cache'i burada
        // siliyoruz ki bir sonraki GetAllAsync çağrısı veritabanından taze
        // veri çeksin ve cache'i doğru içerikle yeniden doldursun.
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(GetCacheKey(currentTenantId));

        var creator = await _context.Users.FindAsync(currentUserId);

        try
        {
            var (aiCategory, aiPriority) = await _aiService.PredictCategoryAndPriorityAsync(ticket.Title, ticket.Description);
            var aiSummary = await _aiService.SummarizeTicketAsync(ticket.Title, ticket.Description);

            ticket.Category = aiCategory;
            ticket.Summary = aiSummary;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AI Service Error] {ex.Message}");
        }

        BackgroundJob.Enqueue<IEmailService>(email => email.SendEmailAsync(
            creator!.Email,
            $"Yeni Ticket Oluşturuldu: {ticket.Title}",
            $"Merhaba {creator.FullName},\n\n\"{ticket.Title}\" başlıklı ticket'ınız başarıyla oluşturuldu.\n\nÖncelik: {ticket.Priority}\nDurum: {ticket.Status}"
        ));

        var response = MapToResponse(ticket, creator?.FullName ?? "", null);

        await _hubContext.Clients.Group($"tenant-{currentTenantId}")
            .SendAsync("ReceiveNewTicket", response);

        return response;
    }

    public async Task<List<TicketResponse>> GetAllAsync(Guid currentTenantId)
    {
        var db = _redis.GetDatabase();
        var cacheKey = GetCacheKey(currentTenantId);
        var cachedData = await db.StringGetAsync(cacheKey);

        if (cachedData.HasValue)
        {
            return JsonSerializer.Deserialize<List<TicketResponse>>((string)cachedData!)!;
        }

        var tickets = await _context.Tickets
            .Where(t => t.TenantId == currentTenantId)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var result = tickets.Select(t => MapToResponse(t, t.CreatedByUser.FullName, t.AssignedToUser?.FullName)).ToList();

        var json = JsonSerializer.Serialize(result);
        await db.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(2));

        return result;
    }

    public async Task<TicketResponse?> GetByIdAsync(Guid ticketId, Guid currentTenantId)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.TenantId == currentTenantId);

        if (ticket is null) return null;

        return MapToResponse(ticket, ticket.CreatedByUser.FullName, ticket.AssignedToUser?.FullName);
    }

    public async Task<TicketResponse> UpdateAsync(Guid ticketId, UpdateTicketRequest request, Guid currentTenantId)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.TenantId == currentTenantId);

        if (ticket is null)
        {
            throw new KeyNotFoundException("Ticket bulunamadı.");
        }

        if (request.Title is not null) ticket.Title = request.Title;
        if (request.Description is not null) ticket.Description = request.Description;
        if (request.Status is not null) ticket.Status = Enum.Parse<TicketStatus>(request.Status, ignoreCase: true);
        if (request.Priority is not null) ticket.Priority = Enum.Parse<TicketPriority>(request.Priority, ignoreCase: true);
        if (request.AssignedToUserId is not null) ticket.AssignedToUserId = request.AssignedToUserId;

        ticket.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Aynı sebep: durum (ya da başka bir alan) değişti, DB güncel ama
        // liste cache'i hâlâ eski veriyi taşıyor. Yine siliyoruz.
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(GetCacheKey(currentTenantId));

        return MapToResponse(ticket, ticket.CreatedByUser.FullName, ticket.AssignedToUser?.FullName);
    }

    private static TicketResponse MapToResponse(Ticket ticket, string createdByName, string? assignedToName)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            CreatedByName = createdByName,
            AssignedToName = assignedToName,
            CreatedAt = ticket.CreatedAt,
            Category = ticket.Category,
            Summary = ticket.Summary
        };
    }
}