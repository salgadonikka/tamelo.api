using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.TaskNotes.Queries.GetTaskNotes;

[Authorize]
public record GetTaskNotesQuery(int TaskItemId) : IRequest<List<TaskNoteDto>>;

public class GetTaskNotesQueryHandler : IRequestHandler<GetTaskNotesQuery, List<TaskNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetTaskNotesQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<TaskNoteDto>> Handle(GetTaskNotesQuery request, CancellationToken cancellationToken)
    {
        return await _context.TaskNotes
            .Where(n => n.TaskItemId == request.TaskItemId && n.TaskItem.UserId == _user.Id)
            .OrderByDescending(n => n.Created)
            .Select(n => new TaskNoteDto(n.Id, n.TaskItemId, n.Content, n.Created, n.LastModified))
            .ToListAsync(cancellationToken);
    }
}
