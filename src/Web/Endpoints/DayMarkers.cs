using Microsoft.AspNetCore.Http.HttpResults;
using Tamelo.Api.Application.DayMarkers.Commands.DeleteDayMarker;
using Tamelo.Api.Application.DayMarkers.Commands.UpsertDayMarker;

namespace Tamelo.Api.Web.Endpoints;

public class DayMarkers : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpsertDayMarker, "{taskItemId}/{date}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteDayMarker, "{taskItemId}/{date}").RequireAuthorization();
    }

    [EndpointName(nameof(UpsertDayMarker))]
    [EndpointSummary("Set a day marker state")]
    [EndpointDescription("Creates or updates the circle state for a task on a specific date (YYYY-MM-DD). Valid states: empty, planned, started, completed.")]
    public async Task<NoContent> UpsertDayMarker(ISender sender, int taskItemId, string date, UpsertDayMarkerRequest body)
    {
        await sender.Send(new UpsertDayMarkerCommand
        {
            TaskItemId = taskItemId,
            Date = date,
            State = body.State
        });
        return TypedResults.NoContent();
    }

    [EndpointName(nameof(DeleteDayMarker))]
    [EndpointSummary("Remove a day marker")]
    [EndpointDescription("Removes the circle state for a task on a specific date (YYYY-MM-DD).")]
    public async Task<NoContent> DeleteDayMarker(ISender sender, int taskItemId, string date)
    {
        await sender.Send(new DeleteDayMarkerCommand(taskItemId, date));
        return TypedResults.NoContent();
    }
}

public record UpsertDayMarkerRequest(string State);
