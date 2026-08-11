using WestDesk.Domain.Common;

namespace WestDesk.Domain.Entities;

public class User : BaseEntity, ITenantEntity 
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
}

public enum UserRole
{
    Admin,
    Agent,
    Customer
}