using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.DayMarkers.Commands.DeleteDayMarker;

[Authorize]
public record DeleteDayMarkerCommand(int TaskItemId, string Date) : IRequest;

public class DeleteDayMarkerCommandHandler : IRequestHandler<DeleteDayMarkerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteDayMarkerCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteDayMarkerCommand request, CancellationToken cancellationToken)
    {
        var marker = await _context.DayMarkers
            .FirstOrDefaultAsync(
                m => m.TaskItemId == request.TaskItemId
                  && m.Date == request.Date
                  && m.TaskItem.UserId == _user.Id,
                cancellationToken);

        if (marker != null)
        {
            _context.DayMarkers.Remove(marker);

            await _context.SaveChangesAsync(cancellationToken);

            _context.TaskHistories.Add(new TaskHistory
            {
                TaskItemId = request.TaskItemId,
                UserId = _user.Id!,
                EventType = "marker_removed",
                FieldName = request.Date,
                OldValue = marker.State.ToString().ToLowerInvariant(),
                NewValue = null,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
