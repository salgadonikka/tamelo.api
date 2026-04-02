using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Projects.Commands.UpdateProject;

[Authorize]
public record UpdateProjectCommand : IRequest
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
    public string? Notes { get; init; }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateProjectCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, project);

        project.Name = request.Name;
        project.Color = request.Color;
        project.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
