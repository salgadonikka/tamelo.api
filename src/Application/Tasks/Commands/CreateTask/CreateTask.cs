using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Application.Tasks.Queries.GetTasks;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.Tasks.Commands.CreateTask;

[Authorize]
public record CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public int? ProjectId { get; init; }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var maxSortOrder = await _context.TaskItems
            .Where(t => t.UserId == _user.Id)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var task = new TaskItem
        {
            UserId = _user.Id!,
            Title = request.Title,
            Notes = request.Notes,
            ProjectId = request.ProjectId,
            SortOrder = maxSortOrder + 1
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new TaskDto(task.Id, task.Title, task.Notes, task.ProjectId, [], task.Created, task.Archived, task.SortOrder);
    }
}
