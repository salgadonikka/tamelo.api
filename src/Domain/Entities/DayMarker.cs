namespace Tamelo.Api.Domain.Entities;

public class DayMarker : BaseEntity
{
    public int TaskItemId { get; set; }
    public string Date { get; set; } = string.Empty; // ISO date YYYY-MM-DD
    public CircleState State { get; set; }

    public TaskItem TaskItem { get; set; } = null!;
}
