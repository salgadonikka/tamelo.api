namespace Tamelo.Api.Application.Projects.Queries.GetProjects;

public record ProjectDto(
    int Id,
    string Name,
    string? Color,
    string? Notes,
    DateTimeOffset CreatedAt);
