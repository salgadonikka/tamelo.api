using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Tasks.Queries.GetTaskHistory;

[Authorize]
public record GetTaskHistoryQuery(int TaskItemId) : IRequest<List<TaskHistoryDto>>;

public record TaskHistoryDto(
    int Id,
    string EventType,
    string? FieldName,
    string? OldValue,
    string? NewValue,
    DateTimeOffset CreatedAt);

public class GetTaskHistoryQueryHandler : IRequestHandler<GetTaskHistoryQuery, List<TaskHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetTaskHistoryQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<TaskHistoryDto>> Handle(GetTaskHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _context.TaskHistories
            .Where(h => h.TaskItemId == request.TaskItemId && h.TaskItem.UserId == _user.Id)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new TaskHistoryDto(h.Id, h.EventType, h.FieldName, h.OldValue, h.NewValue, h.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
