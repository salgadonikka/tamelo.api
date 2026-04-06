using System.Text;
using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Domain.Entities;
using Tamelo.Api.Domain.Enums;

namespace Tamelo.Api.Application.Tasks.Queries.ExportTasks;

[Authorize]
public record ExportTasksQuery(
    List<int>? SelectedProjectIds,
    bool IncludeUnassigned,
    bool IncludeArchived) : IRequest<string>;

public class ExportTasksQueryHandler : IRequestHandler<ExportTasksQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ExportTasksQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<string> Handle(ExportTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TaskItems
            .Include(t => t.Markers)
            .Include(t => t.Project)
            .Where(t => t.UserId == _user.Id);

        if (!request.IncludeArchived)
            query = query.Where(t => !t.Archived);

        bool hasProjectFilter = (request.SelectedProjectIds != null && request.SelectedProjectIds.Count > 0)
                                || request.IncludeUnassigned;
        if (hasProjectFilter)
        {
            query = query.Where(t =>
                (request.IncludeUnassigned && t.ProjectId == null) ||
                (request.SelectedProjectIds != null && request.SelectedProjectIds.Contains(t.ProjectId ?? -1)));
        }

        var tasks = await query.OrderBy(t => t.SortOrder).ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Task Title,Description,Project,Status,Target Date,Archived,Created At");

        foreach (var task in tasks)
        {
            var (status, targetDate) = DeriveStatus(task.Markers);
            sb.AppendLine(string.Join(",", new[]
            {
                Escape(task.Title),
                Escape(task.Notes ?? ""),
                Escape(task.Project?.Name ?? ""),
                status,
                targetDate,
                task.Archived ? "Yes" : "No",
                task.Created.ToString("o"),
            }));
        }

        return sb.ToString();
    }

    private static (string status, string targetDate) DeriveStatus(IEnumerable<DayMarker> markers)
    {
        var list = markers.ToList();
        if (list.Count == 0) return ("unplanned", "");

        var completed = list.Where(m => m.State == CircleState.Completed).MaxBy(m => m.Date);
        if (completed != null) return ("completed", completed.Date);

        var started = list.Where(m => m.State == CircleState.Started).MaxBy(m => m.Date);
        if (started != null) return ("ongoing", started.Date);

        var planned = list.Where(m => m.State == CircleState.Planned).MaxBy(m => m.Date);
        if (planned != null) return ("planned", planned.Date);

        return ("unplanned", "");
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
