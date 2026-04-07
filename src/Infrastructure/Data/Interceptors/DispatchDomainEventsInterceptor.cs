using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tamelo.Api.Domain.Common;

namespace Tamelo.Api.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DispatchDomainEventsInterceptor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // Do not dispatch domain events before SaveChanges completes — dispatching here may cause
        // handlers to use the same DbContext concurrently while EF is still saving, which can
        // lead to disposed/cancellation-related exceptions (observed with Npgsql).
        return base.SavingChanges(eventData, result);

    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        // Extract domain events and dispatch them from a fresh scope in background.
        // Dispatching from the same DbContext can lead to concurrent usage/disposal
        // of the underlying connection (observed with Npgsql).
        var domainEvents = ExtractDomainEvents(eventData.Context);

        if (domainEvents.Count > 0)
        {
            // Avoid fire-and-forget which can run dispatch concurrently with EF internals
            // causing the Npgsql connector to be accessed after disposal. Dispatch
            // synchronously so handlers run in fresh scopes but complete before SavedChanges returns.
            try
            {
                DispatchDomainEventsAsync(domainEvents).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // Swallow/log as appropriate for your domain. We avoid throwing here to
                // not break the save operation; consider injecting a logger to record failures.
            }
        }

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        // Extract domain events and publish them using a fresh scope so handlers
        // do not reuse the DbContext that has just completed saving.
        var domainEvents = ExtractDomainEvents(eventData.Context);

        if (domainEvents.Count > 0)
        {
            await DispatchDomainEventsAsync(domainEvents);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static List<INotification> ExtractDomainEvents(DbContext? context)
    {
        var domainEvents = new List<INotification>();

        if (context == null) return domainEvents;

        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        domainEvents.AddRange(entities.SelectMany(e => e.DomainEvents.Cast<INotification>()));

        // Clear domain events from the tracked entities so they are not re-dispatched
        entities.ForEach(e => e.ClearDomainEvents());

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(List<INotification> domainEvents)
    {
        if (domainEvents == null || domainEvents.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}
