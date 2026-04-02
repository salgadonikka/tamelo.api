namespace Tamelo.Api.Application.TaskNotes.Queries.GetTaskNotes;

public record TaskNoteDto(
    int Id,
    int TaskItemId,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastModified);
