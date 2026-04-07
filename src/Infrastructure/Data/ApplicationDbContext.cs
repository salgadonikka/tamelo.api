using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Domain.Common;
using Tamelo.Api.Domain.Entities;
using Tamelo.Api.Infrastructure.Identity;

namespace Tamelo.Api.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Existing
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    // Tamelo domain
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<DayMarker> DayMarkers => Set<DayMarker>();
    public DbSet<TaskNote> TaskNotes => Set<TaskNote>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<TaskHistory> TaskHistories => Set<TaskHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        //builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply to every entity that inherits BaseAuditableEntity
        foreach (var entityType in builder.Model.GetEntityTypes()
            .Where(e => typeof(BaseAuditableEntity).IsAssignableFrom(e.ClrType)))
        {
            builder.Entity(entityType.ClrType)
                .Property(nameof(BaseAuditableEntity.Created))
                .ValueGeneratedNever();

            builder.Entity(entityType.ClrType)
                .Property(nameof(BaseAuditableEntity.LastModified))
                .ValueGeneratedNever();
        }

        // Apply HiLo to ALL int PKs — eliminates RETURNING on every entity
        builder.UseHiLo();

        // your entity configurations...
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    //private readonly ILogger<ApplicationDbContext> _logger;
    //public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger) : base(options) { _logger = logger; _logger.LogDebug("ApplicationDbContext CREATED {ContextHash}", GetHashCode()); }

    //public override void Dispose() { _logger?.LogWarning("ApplicationDbContext DISPOSED {ContextHash}", GetHashCode()); base.Dispose(); }

    //public override ValueTask DisposeAsync() { _logger.LogWarning("ApplicationDbContext DISPOSED ASYNC {ContextHash}", GetHashCode()); return base.DisposeAsync(); }

}
