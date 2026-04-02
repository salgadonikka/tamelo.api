using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.TaskNotes.Commands.DeleteTaskNote;

[Authorize]
public record DeleteTaskNoteCommand(int Id) : IRequest;

public class DeleteTaskNoteCommandHandler : IRequestHandler<DeleteTaskNoteCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteTaskNoteCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteTaskNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _context.TaskNotes
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.TaskItem.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, note);

        _context.TaskNotes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
