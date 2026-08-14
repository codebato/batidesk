using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using NvnDesk.Application.Interfaces;
using NvnDesk.Domain.Entities;
using NvnDesk.Infrastructure.Persistence;

namespace NvnDesk.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly NvnDeskDbContext _context;
    private readonly IConfiguration _configuration;

    public StripeService(NvnDeskDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;

        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

public async Task<string> CreateCheckoutSessionAsync(Guid tenantId, string tenantEmail)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant is null)
        {
            throw new KeyNotFoundException("Tenant bulunamadı.");
        }

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = _configuration["Stripe:ProPriceId"],
                    Quantity = 1
                }
            },
            SuccessUrl = "http://localhost:5004/api/stripe/success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = "http://localhost:5004/api/stripe/cancel",
            Metadata = new Dictionary<string, string>
            {
                { "tenantId", tenantId.ToString() }
            }
        };

        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            options.Customer = tenant.StripeCustomerId;
        }
        else
        {
            options.CustomerEmail = tenantEmail;
        }

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        return session.Url;
    }

    public async Task HandleSubscriptionActivatedAsync(Guid tenantId, string stripeCustomerId, string stripeSubscriptionId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant is null) return;


        tenant.StripeSubscriptionId = stripeSubscriptionId;
        tenant.Plan = SubscriptionPlan.Pro;

        await _context.SaveChangesAsync();
    }

public async Task HandleSubscriptionCancelledAsync(string stripeSubscriptionId)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.StripeSubscriptionId == stripeSubscriptionId);

        if (tenant is null) return;

        tenant.Plan = SubscriptionPlan.Free;
        tenant.StripeSubscriptionId = null;

        await _context.SaveChangesAsync();
    }
}