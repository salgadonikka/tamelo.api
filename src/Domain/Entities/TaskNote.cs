namespace Tamelo.Api.Domain.Entities;

public class TaskNote : BaseAuditableEntity
{
    public int TaskItemId { get; set; }
    public string Content { get; set; } = string.Empty;

    public TaskItem TaskItem { get; set; } = null!;
}
