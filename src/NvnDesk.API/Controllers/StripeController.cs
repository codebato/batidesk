using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NvnDesk.Application.Interfaces;
using Stripe;

namespace NvnDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeController : ControllerBase
{
    private readonly IStripeService _stripeService;

    public StripeController(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirstValue("tenantId");
        return Guid.Parse(tenantIdClaim!);
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email)!;
    }

    // 📌 Bu endpoint'e [Authorize] koyduk çünkü sadece giriş yapmış bir tenant
    // kendi aboneliğini yükseltebilir, herkes çağıramaz.
    [Authorize]
    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        var url = await _stripeService.CreateCheckoutSessionAsync(GetCurrentTenantId(), GetCurrentUserEmail());

        return Ok(new { checkoutUrl = url });
    }


    [AllowAnonymous]
    [HttpGet("success")]
    public IActionResult Success([FromQuery] string session_id)
    {
        return Ok(new { message = "Ödeme başarılı, teşekkürler!", sessionId = session_id });
    }

    [AllowAnonymous]
    [HttpGet("cancel")]
    public IActionResult Cancel()
    {
        return Ok(new { message = "Ödeme iptal edildi." });
    }


    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {

        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Stripe:WebhookSecret"]
            );


            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                if (session is not null && session.Metadata.TryGetValue("tenantId", out var tenantIdStr))
                {
                    var tenantId = Guid.Parse(tenantIdStr);
                    var customerId = session.CustomerId;
                    var subscriptionId = session.SubscriptionId;

                    var stripeService = HttpContext.RequestServices.GetRequiredService<IStripeService>();
                    await stripeService.HandleSubscriptionActivatedAsync(tenantId, customerId, subscriptionId);


                }
            }
            else if (stripeEvent.Type == "customer.subscription.deleted")
            {
                var subscription = stripeEvent.Data.Object as Subscription;

                if (subscription is not null)
                {
                    await _stripeService.HandleSubscriptionCancelledAsync(subscription.Id);
                }

            }


            return Ok();
        }
        catch (StripeException e)
        {

            return BadRequest(new { error = e.Message });
        }
    }
}