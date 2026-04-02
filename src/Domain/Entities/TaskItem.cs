namespace Tamelo.Api.Domain.Entities;

public class TaskItem : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? ProjectId { get; set; }
    public bool Archived { get; set; }
    public int SortOrder { get; set; }

    public Project? Project { get; set; }
    public IList<DayMarker> Markers { get; private set; } = new List<DayMarker>();
    public IList<TaskNote> TaskNotes { get; private set; } = new List<TaskNote>();
}
