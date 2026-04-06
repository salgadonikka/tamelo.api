using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Tamelo.Api.Application.Tasks.Commands.AddTaskHistoryEvent;
using Tamelo.Api.Application.Tasks.Commands.ArchiveTask;
using Tamelo.Api.Application.Tasks.Commands.CreateTask;
using Tamelo.Api.Application.Tasks.Commands.DeleteTask;
using Tamelo.Api.Application.Tasks.Commands.ImportTasks;
using Tamelo.Api.Application.Tasks.Commands.ReorderTask;
using Tamelo.Api.Application.Tasks.Commands.UpdateTask;
using Tamelo.Api.Application.Tasks.Queries.ExportTasks;
using Tamelo.Api.Application.Tasks.Queries.GetTask;
using Tamelo.Api.Application.Tasks.Queries.GetTaskHistory;
using Tamelo.Api.Application.Tasks.Queries.GetTasks;

namespace Tamelo.Api.Web.Endpoints;

public class Tasks : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTasks).RequireAuthorization();
        groupBuilder.MapGet(ExportTasks, "export").RequireAuthorization();
        groupBuilder.MapGet(GetTask, "{id}").RequireAuthorization();
        groupBuilder.MapPost(CreateTask).RequireAuthorization();
        groupBuilder.MapPut(UpdateTask, "{id}").RequireAuthorization();
        groupBuilder.MapPatch(ArchiveTask, "{id}/archive").RequireAuthorization();
        groupBuilder.MapPatch(ReorderTask, "{id}/reorder").RequireAuthorization();
        groupBuilder.MapDelete(DeleteTask, "{id}").RequireAuthorization();
        groupBuilder.MapPost(ImportTasks, "import").RequireAuthorization();
        groupBuilder.MapGet(GetTaskHistory, "{id}/history").RequireAuthorization();
        groupBuilder.MapPost(AddTaskHistoryEvent, "{id}/history").RequireAuthorization();
    }

    [EndpointName(nameof(GetTasks))]
    [EndpointSummary("Get all tasks")]
    [EndpointDescription("Returns all tasks for the authenticated user, including day markers. Pass includeArchived=true to include archived tasks.")]
    public async Task<Ok<List<TaskDto>>> GetTasks(ISender sender, bool includeArchived = false)
    {
        var result = await sender.Send(new GetTasksQuery(includeArchived));
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(GetTask))]
    [EndpointSummary("Get a task by ID")]
    public async Task<Ok<TaskDto>> GetTask(ISender sender, int id)
    {
        var result = await sender.Send(new GetTaskQuery(id));
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(CreateTask))]
    [EndpointSummary("Create a new task")]
    public async Task<Created<TaskDto>> CreateTask(ISender sender, CreateTaskCommand command)
    {
        var dto = await sender.Send(command);
        return TypedResults.Created($"/{nameof(Tasks)}/{dto.Id}", dto);
    }

    [EndpointName(nameof(UpdateTask))]
    [EndpointSummary("Update a task")]
    public async Task<Results<NoContent, BadRequest>> UpdateTask(ISender sender, int id, UpdateTaskCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        await sender.Send(command);
        return TypedResults.NoContent();
    }

    [EndpointName(nameof(ArchiveTask))]
    [EndpointSummary("Archive or unarchive a task")]
    public async Task<NoContent> ArchiveTask(ISender sender, int id, ArchiveTaskRequest body)
    {
        await sender.Send(new ArchiveTaskCommand(id, body.Archived));
        return TypedResults.NoContent();
    }

    [EndpointName(nameof(ReorderTask))]
    [EndpointSummary("Reorder a task")]
    [EndpointDescription("Updates the sort order and optionally moves the task to a different project.")]
    public async Task<NoContent> ReorderTask(ISender sender, int id, ReorderTaskCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    [EndpointName(nameof(DeleteTask))]
    [EndpointSummary("Delete a task")]
    public async Task<NoContent> DeleteTask(ISender sender, int id)
    {
        await sender.Send(new DeleteTaskCommand(id));
        return TypedResults.NoContent();
    }
    [EndpointName(nameof(ExportTasks))]
    [EndpointSummary("Export tasks as CSV")]
    [EndpointDescription("Returns a CSV file for the selected projects. Omit project filters to export all tasks.")]
    public async Task<IResult> ExportTasks(
        ISender sender,
        string? projectIds = null,
        bool includeUnassigned = true,
        bool includeArchived = false)
    {
        List<int>? ids = null;
        if (!string.IsNullOrWhiteSpace(projectIds))
        {
            var parts = projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var parsed = new List<int>(parts.Length);
            foreach (var part in parts)
            {
                if (!int.TryParse(part.Trim(), out var id))
                    return Results.BadRequest($"Invalid project ID: '{part.Trim()}'");
                parsed.Add(id);
            }
            ids = parsed;
        }

        var csv = await sender.Send(new ExportTasksQuery(ids, includeUnassigned, includeArchived));
        var bytes = Encoding.UTF8.GetBytes(csv);
        var fileName = $"tasks_export_{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return Results.File(bytes, "text/csv", fileName);
    }

    [EndpointName(nameof(ImportTasks))]
    [EndpointSummary("Import tasks from CSV rows")]
    [EndpointDescription("Accepts parsed CSV rows as JSON and creates tasks. Returns the count of imported and rejected rows.")]
    public async Task<Ok<ImportTasksResult>> ImportTasks(ISender sender, ImportTasksCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(GetTaskHistory))]
    [EndpointSummary("Get history for a task")]
    [EndpointDescription("Returns all activity events for the task, sorted newest first.")]
    public async Task<Ok<List<TaskHistoryDto>>> GetTaskHistory(ISender sender, int id)
    {
        var result = await sender.Send(new GetTaskHistoryQuery(id));
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(AddTaskHistoryEvent))]
    [EndpointSummary("Record a task history event")]
    public async Task<Created<TaskHistoryDto>> AddTaskHistoryEvent(ISender sender, int id, AddTaskHistoryEventCommand command)
    {
        var dto = await sender.Send(command with { TaskItemId = id });
        return TypedResults.Created($"/{nameof(Tasks)}/{id}/history/{dto.Id}", dto);
    }
}

public record ArchiveTaskRequest(bool Archived);
