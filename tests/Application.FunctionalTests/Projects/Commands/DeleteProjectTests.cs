using Tamelo.Api.Application.Common.Exceptions;
using Tamelo.Api.Application.Projects.Commands.CreateProject;
using Tamelo.Api.Application.Projects.Commands.DeleteProject;
using Tamelo.Api.Domain.Entities;


namespace Tamelo.Api.Application.FunctionalTests.Projects.Commands;

public class DeleteProjectTests : BaseTestFixture
{
    [Test]
    public async Task DeleteProject_WithInvalidId_ThrowsNotFoundException()
    {
        await RunAsDefaultUserAsync();

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new DeleteProjectCommand(999999)));
    }

    [Test]
    public async Task DeleteProject_BelongingToAnotherUser_ThrowsNotFoundException()
    {
        // User A creates the project
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new CreateProjectCommand
        {
            Name = "User A's Project"
        });

        // User B attempts to delete it
        await RunAsUserAsync("userb@local", "Testing1234!", Array.Empty<string>());

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new DeleteProjectCommand(result.Id)));
    }

    [Test]
    public async Task DeleteProject_WithValidId_DeletesProject()
    {
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new CreateProjectCommand
        {
            Name = "Project To Delete"
        });

        await SendAsync(new DeleteProjectCommand(result.Id));

        var project = await FindAsync<Project>(result.Id);

        project.ShouldBeNull();
    }
}
