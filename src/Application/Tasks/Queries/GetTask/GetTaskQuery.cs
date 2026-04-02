using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Application.Tasks.Queries.GetTasks;

namespace Tamelo.Api.Application.Tasks.Queries.GetTask;

[Authorize]
public record GetTaskQuery(int Id) : IRequest<TaskDto>;

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetTaskQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<TaskDto> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .Where(t => t.UserId == _user.Id && t.Id == request.Id)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Notes,
                t.ProjectId,
                t.Markers.Select(m => new DayMarkerDto(m.Date, m.State.ToString().ToLower())).ToList(),
                t.Created,
                t.Archived,
                t.SortOrder))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, task);

        return task;
    }
}
