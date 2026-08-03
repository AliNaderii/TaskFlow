using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Authentication;
namespace TaskFlow.Infrastructure.Persistence;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    , IUnitOfWork
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher) : base(options)
    {
        _currentTenant = currentTenant;
        _domainEventDispatcher = domainEventDispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
        ApplyTenantFilters(modelBuilder);
    }

    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Notification> Notifications => Set<Notification>();

    private void ApplyTenantFilters(
        ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model
            .GetEntityTypes()
            .Where(x => typeof(ITenantEntity).IsAssignableFrom(x.ClrType) &&
                       x.ClrType.Name != nameof(User) &&
                       x.ClrType.Name != nameof(Organization)))
        {
            var method = typeof(ApplicationDbContext)
                .GetMethod(
                    nameof(SetTenantFilter),
                    BindingFlags.NonPublic |
                    BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    private void SetTenantFilter<T> (ModelBuilder modelBuilder)
        where T : class, ITenantEntity
    {
        modelBuilder.Entity<T>()
            .HasQueryFilter(
                entity => _currentTenant.OrganizationId == null
                    || entity.OrganizationId == _currentTenant.OrganizationId);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (domainEvents.Any())
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            domainEntities.ForEach(e => e.Entity.ClearDomainEvents());
        }

        return result;
    }
}
