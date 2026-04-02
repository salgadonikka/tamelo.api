using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Application.Projects.Queries.GetProjects;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.Projects.Commands.CreateProject;

[Authorize]
public record CreateProjectCommand : IRequest<ProjectDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
    public string? Notes { get; init; }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateProjectCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            UserId = _user.Id!,
            Name = request.Name,
            Color = request.Color,
            Notes = request.Notes
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectDto(project.Id, project.Name, project.Color, project.Notes, project.Created);
    }
}
