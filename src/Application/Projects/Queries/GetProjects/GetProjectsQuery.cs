using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.Projects.Queries.GetProjects;

[Authorize]
public record GetProjectsQuery : IRequest<List<ProjectDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetProjectsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .Where(p => p.UserId == _user.Id)
            .OrderBy(p => p.Created)
            .Select(p => new ProjectDto(p.Id, p.Name, p.Color, p.Notes, p.Created))
            .ToListAsync(cancellationToken);
    }
}
