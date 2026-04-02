using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Tasks.Commands.DeleteTask;

[Authorize]
public record DeleteTaskCommand(int Id) : IRequest;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, task);

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
