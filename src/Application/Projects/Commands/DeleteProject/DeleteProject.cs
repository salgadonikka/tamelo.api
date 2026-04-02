using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Projects.Commands.DeleteProject;

[Authorize]
public record DeleteProjectCommand(int Id) : IRequest;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteProjectCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, project);

        // Unassign tasks belonging to this project
        var tasks = await _context.TaskItems
            .Where(t => t.ProjectId == request.Id && t.UserId == _user.Id)
            .ToListAsync(cancellationToken);

        foreach (var task in tasks)
            task.ProjectId = null;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
