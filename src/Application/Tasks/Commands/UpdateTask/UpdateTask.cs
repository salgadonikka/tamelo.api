using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Tasks.Commands.UpdateTask;

[Authorize]
public record UpdateTaskCommand : IRequest
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public int? ProjectId { get; init; }
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, task);

        task.Title = request.Title;
        task.Notes = request.Notes;
        task.ProjectId = request.ProjectId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
