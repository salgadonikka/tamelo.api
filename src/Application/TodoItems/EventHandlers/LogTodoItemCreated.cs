using Microsoft.Extensions.Logging;
using Tamelo.Api.Domain.Events;

namespace Tamelo.Api.Application.TodoItems.EventHandlers;

public class LogTodoItemCreated : INotificationHandler<TodoItemCreatedEvent>
{
    private readonly ILogger<LogTodoItemCreated> _logger;

    public LogTodoItemCreated(ILogger<LogTodoItemCreated> logger)
    {
        _logger = logger;
    }

    public Task Handle(TodoItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tamelo.Api Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
