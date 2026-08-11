using WestDesk.Domain.Common;

namespace WestDesk.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public bool IsActive { get; set; } = true;

    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

public enum SubscriptionPlan
{
    Free,
    Pro
}