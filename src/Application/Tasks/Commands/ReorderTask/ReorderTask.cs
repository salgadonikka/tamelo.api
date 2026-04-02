using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;

namespace Tamelo.Api.Application.Tasks.Commands.ReorderTask;

[Authorize]
public record ReorderTaskCommand : IRequest
{
    public int Id { get; init; }
    public int SortOrder { get; init; }
    public int? ProjectId { get; init; }
}

public class ReorderTaskCommandHandler : IRequestHandler<ReorderTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ReorderTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ReorderTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == _user.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, task);

        task.SortOrder = request.SortOrder;
        task.ProjectId = request.ProjectId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
