using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Application.Tasks.Queries.GetTaskHistory;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.Tasks.Commands.AddTaskHistoryEvent;

[Authorize]
public record AddTaskHistoryEventCommand : IRequest<TaskHistoryDto>
{
    public int TaskItemId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? FieldName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}

public class AddTaskHistoryEventCommandHandler : IRequestHandler<AddTaskHistoryEventCommand, TaskHistoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AddTaskHistoryEventCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<TaskHistoryDto> Handle(AddTaskHistoryEventCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == request.TaskItemId && t.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.TaskItemId, task);

        var entry = new TaskHistory
        {
            TaskItemId = request.TaskItemId,
            UserId = _user.Id!,
            EventType = request.EventType,
            FieldName = request.FieldName,
            OldValue = request.OldValue,
            NewValue = request.NewValue,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _context.TaskHistories.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return new TaskHistoryDto(entry.Id, entry.EventType, entry.FieldName, entry.OldValue, entry.NewValue, entry.CreatedAt);
    }
}
