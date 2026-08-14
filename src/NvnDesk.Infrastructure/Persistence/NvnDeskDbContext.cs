using Microsoft.EntityFrameworkCore;
using NvnDesk.Domain.Common;
using NvnDesk.Domain.Entities;

namespace NvnDesk.Infrastructure.Persistence;

public class NvnDeskDbContext : DbContext
{

    private readonly Guid? _currentTenantId;

    public NvnDeskDbContext(DbContextOptions<NvnDeskDbContext> options, Guid? currentTenantId = null)
    : base(options)
    {
        _currentTenantId = currentTenantId;

    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasQueryFilter(u => _currentTenantId == null || u.TenantId == _currentTenantId);

        modelBuilder.Entity<Ticket>()
            .HasQueryFilter(t => _currentTenantId == null || t.TenantId == _currentTenantId); 
        modelBuilder.Entity<Ticket>()
        .HasOne(t => t.CreatedByUser)
        .WithMany()
        .HasForeignKey(t => t.CreatedByUserId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
                  .HasOne(t => t.AssignedToUser)
                  .WithMany()
                  .HasForeignKey(t => t.AssignedToUserId)
                  .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
        .HasIndex(u => new { u.TenantId, u.Email })
        .IsUnique();



    }
}