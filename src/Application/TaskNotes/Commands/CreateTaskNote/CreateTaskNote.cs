using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Application.TaskNotes.Queries.GetTaskNotes;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.TaskNotes.Commands.CreateTaskNote;

[Authorize]
public record CreateTaskNoteCommand : IRequest<TaskNoteDto>
{
    public int TaskItemId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public class CreateTaskNoteCommandHandler : IRequestHandler<CreateTaskNoteCommand, TaskNoteDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateTaskNoteCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<TaskNoteDto> Handle(CreateTaskNoteCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == request.TaskItemId && t.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.TaskItemId, task);

        var note = new TaskNote
        {
            TaskItemId = request.TaskItemId,
            Content = request.Content
        };

        _context.TaskNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return new TaskNoteDto(note.Id, note.TaskItemId, note.Content, note.Created, note.LastModified);
    }
}
