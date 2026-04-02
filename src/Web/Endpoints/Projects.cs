using Microsoft.AspNetCore.Http.HttpResults;
using Tamelo.Api.Application.Projects.Commands.CreateProject;
using Tamelo.Api.Application.Projects.Commands.DeleteProject;
using Tamelo.Api.Application.Projects.Commands.UpdateProject;
using Tamelo.Api.Application.Projects.Queries.GetProject;
using Tamelo.Api.Application.Projects.Queries.GetProjects;

namespace Tamelo.Api.Web.Endpoints;

public class Projects : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProjects).RequireAuthorization();
        groupBuilder.MapGet(GetProject, "{id}").RequireAuthorization();
        groupBuilder.MapPost(CreateProject).RequireAuthorization();
        groupBuilder.MapPut(UpdateProject, "{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteProject, "{id}").RequireAuthorization();
    }

    [EndpointName(nameof(GetProjects))]
    [EndpointSummary("Get all projects")]
    [EndpointDescription("Returns all projects for the authenticated user.")]
    public async Task<Ok<List<ProjectDto>>> GetProjects(ISender sender)
    {
        var result = await sender.Send(new GetProjectsQuery());
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(GetProject))]
    [EndpointSummary("Get a project by ID")]
    public async Task<Ok<ProjectDto>> GetProject(ISender sender, int id)
    {
        var result = await sender.Send(new GetProjectQuery(id));
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(CreateProject))]
    [EndpointSummary("Create a new project")]
    public async Task<Created<ProjectDto>> CreateProject(ISender sender, CreateProjectCommand command)
    {
        var dto = await sender.Send(command);
        return TypedResults.Created($"/{nameof(Projects)}/{dto.Id}", dto);
    }

    [EndpointName(nameof(UpdateProject))]
    [EndpointSummary("Update a project")]
    public async Task<Results<NoContent, BadRequest>> UpdateProject(ISender sender, int id, UpdateProjectCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        await sender.Send(command);
        return TypedResults.NoContent();
    }

    [EndpointName(nameof(DeleteProject))]
    [EndpointSummary("Delete a project")]
    [EndpointDescription("Deletes the project and unassigns all tasks that belonged to it.")]
    public async Task<NoContent> DeleteProject(ISender sender, int id)
    {
        await sender.Send(new DeleteProjectCommand(id));
        return TypedResults.NoContent();
    }
}
