using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Tasks.Commands.ArchiveTask;

[Authorize]
public record ArchiveTaskCommand(int Id, bool Archived = true) : IRequest;

public class ArchiveTaskCommandHandler : IRequestHandler<ArchiveTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ArchiveTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ArchiveTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, task);

        task.Archived = request.Archived;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
