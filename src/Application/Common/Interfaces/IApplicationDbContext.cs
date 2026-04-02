using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Existing
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }

    // Tamelo domain
    DbSet<Project> Projects { get; }
    DbSet<TaskItem> TaskItems { get; }
    DbSet<DayMarker> DayMarkers { get; }
    DbSet<TaskNote> TaskNotes { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<TaskHistory> TaskHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
