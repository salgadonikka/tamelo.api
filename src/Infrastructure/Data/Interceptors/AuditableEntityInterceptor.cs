using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Domain.Common;

namespace Tamelo.Api.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _dateTime;

    public AuditableEntityInterceptor(
        IUser user,
        TimeProvider dateTime)
    {
        _user = user;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    //public void UpdateEntities(DbContext? context)
    //{
    //    if (context == null) return;

    //    foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
    //    {
    //        if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
    //        {
    //            var utcNow = _dateTime.GetUtcNow();
    //            if (entry.State == EntityState.Added)
    //            {
    //                entry.Entity.CreatedBy = _user.Id;
    //                entry.Entity.Created = utcNow;
    //            }
    //            entry.Entity.LastModifiedBy = _user.Id;
    //            entry.Entity.LastModified = utcNow;
    //        }
    //    }
    //}

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            // Only process entities that were explicitly Added or Modified
            // Remove HasChangedOwnedEntities() — it causes parent to be touched
            // when only a child/owned entity changed
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var utcNow = _dateTime.GetUtcNow();
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.Created = utcNow;
                }
                entry.Entity.LastModifiedBy = _user.Id;
                entry.Entity.LastModified = utcNow;
            }
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
