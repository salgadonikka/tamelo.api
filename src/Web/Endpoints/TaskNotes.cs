using Microsoft.AspNetCore.Http.HttpResults;
using Tamelo.Api.Application.TaskNotes.Commands.CreateTaskNote;
using Tamelo.Api.Application.TaskNotes.Commands.DeleteTaskNote;
using Tamelo.Api.Application.TaskNotes.Commands.UpdateTaskNote;
using Tamelo.Api.Application.TaskNotes.Queries.GetTaskNotes;

namespace Tamelo.Api.Web.Endpoints;

public class TaskNotes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTaskNotes, "{taskItemId}").RequireAuthorization();
        groupBuilder.MapPost(CreateTaskNote).RequireAuthorization();
        groupBuilder.MapPut(UpdateTaskNote, "{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteTaskNote, "{id}").RequireAuthorization();
    }

    [EndpointName(nameof(GetTaskNotes))]
    [EndpointSummary("Get notes for a task")]
    public async Task<Ok<List<TaskNoteDto>>> GetTaskNotes(ISender sender, int taskItemId)
    {
        var result = await sender.Send(new GetTaskNotesQuery(taskItemId));
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(CreateTaskNote))]
    [EndpointSummary("Add a note to a task")]
    public async Task<Created<TaskNoteDto>> CreateTaskNote(ISender sender, CreateTaskNoteCommand command)
    {
        var dto = await sender.Send(command);
        return TypedResults.Created($"/{nameof(TaskNotes)}/{dto.Id}", dto);
    }

    [EndpointName(nameof(UpdateTaskNote))]
    [EndpointSummary("Update a task note")]
    public async Task<Results<NoContent, BadRequest>> UpdateTaskNote(ISender sender, int id, UpdateTaskNoteCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        await sender.Send(command);
        return TypedResults.NoContent();
    }

    [EndpointName(nameof(DeleteTaskNote))]
    [EndpointSummary("Delete a task note")]
    public async Task<NoContent> DeleteTaskNote(ISender sender, int id)
    {
        await sender.Send(new DeleteTaskNoteCommand(id));
        return TypedResults.NoContent();
    }
}
