namespace Tamelo.Api.Application.Tasks.Queries.GetTasks;

public record TaskDto(
    int Id,
    string Title,
    string? Notes,
    int? ProjectId,
    List<DayMarkerDto> Markers,
    DateTimeOffset CreatedAt,
    bool Archived,
    int SortOrder);
