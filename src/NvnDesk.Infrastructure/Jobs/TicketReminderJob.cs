using Microsoft.EntityFrameworkCore;
using NvnDesk.Application.Services;
using NvnDesk.Domain.Entities; 
using NvnDesk.Infrastructure.Persistence;


namespace NvnDesk.Infrastructure.Jobs
{

    public class TicketReminderJob
    {
        private readonly NvnDeskDbContext _context;
        private readonly IEmailService _emailService;

        public TicketReminderJob(NvnDeskDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        
        public async Task SendStaleTicketRemindersAsync()
        {
            
            var thresholdDate = DateTime.UtcNow.AddHours(-24);

            var staleTickets = await _context.Tickets
                .Where(t => t.Status == TicketStatus.Open && t.CreatedAt <= thresholdDate)
                .Include(t => t.CreatedByUser) 
                .ToListAsync();

            
            foreach (var ticket in staleTickets)
            {
                await _emailService.SendEmailAsync(
                    ticket.CreatedByUser.Email,
                    $"Hatırlatma: \"{ticket.Title}\" hâlâ açık",
                    $"Merhaba {ticket.CreatedByUser.FullName},\n\n\"{ticket.Title}\" başlıklı ticket'ınız 24 saatten uzun süredir açık durumda. Ekibimiz en kısa sürede ilgilenecektir."
                );
            }
        }
    }
}