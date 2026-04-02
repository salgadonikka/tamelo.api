using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Application.Projects.Queries.GetProjects;

namespace Tamelo.Api.Application.Projects.Queries.GetProject;

[Authorize]
public record GetProjectQuery(int Id) : IRequest<ProjectDto>;

public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, ProjectDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetProjectQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<ProjectDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Where(p => p.UserId == _user.Id && p.Id == request.Id)
            .Select(p => new ProjectDto(p.Id, p.Name, p.Color, p.Notes, p.Created))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, project);

        return project;
    }
}
