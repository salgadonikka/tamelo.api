using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Domain.Entities;
using Tamelo.Api.Domain.Enums;

namespace Tamelo.Api.Application.Tasks.Commands.ImportTasks;

public record ImportTaskRow
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Project { get; init; }
    public string Status { get; init; } = "unplanned";
    public string? TargetDate { get; init; }
    public bool Archived { get; init; }
}

[Authorize]
public record ImportTasksCommand : IRequest<ImportTasksResult>
{
    public List<ImportTaskRow> Rows { get; init; } = [];
    public string UnknownProjectAction { get; init; } = "reject"; // "create" or "reject"
}

public record ImportTasksResult(int Imported, int Rejected);

public class ImportTasksCommandHandler : IRequestHandler<ImportTasksCommand, ImportTasksResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ImportTasksCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    private static readonly string[] ProjectColors =
    [
        "hsl(150, 25%, 40%)", "hsl(200, 40%, 50%)", "hsl(15, 45%, 55%)",
        "hsl(280, 30%, 50%)", "hsl(45, 50%, 50%)"
    ];

    public async Task<ImportTasksResult> Handle(ImportTasksCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        var existingProjects = await _context.Projects
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        var existingTaskKeys = await _context.TaskItems
            .Where(t => t.UserId == userId)
            .Select(t => new { t.Title, t.ProjectId })
            .ToListAsync(cancellationToken);

        var maxSortOrder = await _context.TaskItems
            .Where(t => t.UserId == userId)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var projectCache = existingProjects.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        var taskSet = existingTaskKeys.Select(t => (t.Title.ToLowerInvariant(), t.ProjectId)).ToHashSet();

        // Pre-pass: create all new projects in one batch so they get IDs before task processing.
        if (request.UnknownProjectAction == "create")
        {
            var newProjectNames = request.Rows
                .Where(r => !string.IsNullOrWhiteSpace(r.Project) && !projectCache.ContainsKey(r.Project!))
                .Select(r => r.Project!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var name in newProjectNames)
            {
                var project = new Project
                {
                    UserId = userId,
                    Name = name,
                    Color = ProjectColors[projectCache.Count % ProjectColors.Length],
                };
                _context.Projects.Add(project);
                projectCache[name] = project;
            }

            if (newProjectNames.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }

        // Main pass: accumulate all tasks, markers, and history — save once at the end.
        int imported = 0;
        int rejected = 0;

        foreach (var row in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.Title))
            {
                rejected++;
                continue;
            }

            // Resolve project
            Project? project = null;
            if (!string.IsNullOrWhiteSpace(row.Project))
            {
                if (!projectCache.TryGetValue(row.Project, out project))
                {
                    rejected++;
                    continue;
                }
            }

            // Duplicate check: same title (case-insensitive) + same project
            if (taskSet.Contains((row.Title.ToLowerInvariant(), project?.Id)))
            {
                rejected++;
                continue;
            }

            var task = new TaskItem
            {
                UserId = userId,
                Title = row.Title,
                Notes = string.IsNullOrWhiteSpace(row.Description) ? null : row.Description,
                ProjectId = project?.Id,
                Archived = row.Archived,
                SortOrder = ++maxSortOrder,
            };
            _context.TaskItems.Add(task);

            // Add day marker for status (except unplanned)
            if (!string.IsNullOrWhiteSpace(row.TargetDate) && row.Status != "unplanned")
            {
                var state = row.Status switch
                {
                    "planned"   => CircleState.Planned,
                    "ongoing"   => CircleState.Started,
                    "completed" => CircleState.Completed,
                    _           => CircleState.Empty,
                };

                if (state != CircleState.Empty)
                {
                    // Use navigation property so EF resolves the FK after task is inserted.
                    _context.DayMarkers.Add(new DayMarker
                    {
                        TaskItem = task,
                        Date = row.TargetDate,
                        State = state,
                    });
                }
            }

            // Use navigation property so EF resolves the FK after task is inserted.
            _context.TaskHistories.Add(new TaskHistory
            {
                TaskItem = task,
                UserId = userId,
                EventType = "created",
                NewValue = task.Title,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            taskSet.Add((row.Title.ToLowerInvariant(), project?.Id));
            imported++;
        }

        if (imported > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return new ImportTasksResult(imported, rejected);
    }
}
