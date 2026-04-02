using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.TaskNotes.Commands.UpdateTaskNote;

[Authorize]
public record UpdateTaskNoteCommand : IRequest
{
    public int Id { get; init; }
    public string Content { get; init; } = string.Empty;
}

public class UpdateTaskNoteCommandHandler : IRequestHandler<UpdateTaskNoteCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateTaskNoteCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateTaskNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _context.TaskNotes
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.TaskItem.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, note);

        note.Content = request.Content;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
