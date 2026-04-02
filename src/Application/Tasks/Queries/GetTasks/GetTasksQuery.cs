using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Tasks.Queries.GetTasks;

[Authorize]
public record GetTasksQuery(bool IncludeArchived = false) : IRequest<List<TaskDto>>;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetTasksQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TaskItems
            .Where(t => t.UserId == _user.Id);

        if (!request.IncludeArchived)
            query = query.Where(t => !t.Archived);

        return await query
            .OrderBy(t => t.SortOrder)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Notes,
                t.ProjectId,
                t.Markers.Select(m => new DayMarkerDto(m.Date, m.State.ToString().ToLower())).ToList(),
                t.Created,
                t.Archived,
                t.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
