namespace Tamelo.Api.Domain.Entities;

public class Project : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Notes { get; set; }

    public IList<TaskItem> Tasks { get; private set; } = new List<TaskItem>();
}
