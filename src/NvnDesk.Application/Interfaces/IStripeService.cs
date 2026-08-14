namespace NvnDesk.Application.Interfaces;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(Guid tenantId, string tenantEmail);
    Task HandleSubscriptionActivatedAsync(Guid tenantId, string stripeCustomerId, string stripeSubscriptionId);
    Task HandleSubscriptionCancelledAsync(string stripeSubscriptionId);
}