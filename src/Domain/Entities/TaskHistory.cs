namespace Tamelo.Api.Domain.Entities;

public class TaskHistory : BaseEntity
{
    public int TaskItemId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public TaskItem TaskItem { get; set; } = null!;
}
