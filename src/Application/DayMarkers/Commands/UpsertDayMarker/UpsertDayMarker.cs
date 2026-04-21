using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Domain.Entities;
using Tamelo.Api.Domain.Enums;

namespace Tamelo.Api.Application.DayMarkers.Commands.UpsertDayMarker;

[Authorize]
public record UpsertDayMarkerCommand : IRequest
{
    public int TaskItemId { get; init; }
    public string Date { get; init; } = string.Empty; // ISO date YYYY-MM-DD
    public string State { get; init; } = string.Empty; // empty | planned | started | completed
}

public class UpsertDayMarkerCommandHandler : IRequestHandler<UpsertDayMarkerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpsertDayMarkerCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpsertDayMarkerCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == request.TaskItemId && t.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.TaskItemId, task);

        var state = Enum.Parse<CircleState>(request.State, ignoreCase: true);

        var marker = await _context.DayMarkers
            .FirstOrDefaultAsync(m => m.TaskItemId == request.TaskItemId && m.Date == request.Date, cancellationToken);

        var isNew = marker == null;
        var oldState = marker?.State;

        if (isNew)
        {
            _context.DayMarkers.Add(new DayMarker
            {
                TaskItemId = request.TaskItemId,
                Date = request.Date,
                State = state
            });
        }
        else
        {
            marker!.State = state;
        }

        _context.TaskHistories.Add(new TaskHistory
        {
            TaskItemId = request.TaskItemId,
            UserId = _user.Id!,
            EventType = isNew ? "marker_set" : "marker_updated",
            FieldName = request.Date,
            OldValue = oldState.HasValue ? oldState.Value.ToString().ToLowerInvariant() : null,
            NewValue = request.State.ToLowerInvariant(),
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _context.SaveChangesAsync(cancellationToken);

    }
}
