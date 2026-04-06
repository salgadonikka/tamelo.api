using Tamelo.Api.Application.Common.Exceptions;
using Tamelo.Api.Application.Projects.Commands.CreateProject;
using Tamelo.Api.Application.Projects.Commands.UpdateProject;
using Tamelo.Api.Domain.Entities;


namespace Tamelo.Api.Application.FunctionalTests.Projects.Commands;

public class UpdateProjectTests : BaseTestFixture
{
    [Test]
    public async Task UpdateProject_WithInvalidId_ThrowsNotFoundException()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateProjectCommand
        {
            Id = 999999,
            Name = "Updated Name"
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task UpdateProject_BelongingToAnotherUser_ThrowsNotFoundException()
    {
        // User A creates the project
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new CreateProjectCommand
        {
            Name = "User A's Project"
        });

        // User B attempts to update it
        await RunAsUserAsync("userb@local", "Testing1234!", Array.Empty<string>());

        var command = new UpdateProjectCommand
        {
            Id = result.Id,
            Name = "Hijacked Name"
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task UpdateProject_WithValidData_UpdatesProject()
    {
        await RunAsDefaultUserAsync();

        var created = await SendAsync(new CreateProjectCommand
        {
            Name = "Original Name",
            Color = "#111111",
            Notes = "Original notes"
        });

        var updateCommand = new UpdateProjectCommand
        {
            Id = created.Id,
            Name = "Updated Name",
            Color = "#222222",
            Notes = "Updated notes"
        };

        await SendAsync(updateCommand);

        var project = await FindAsync<Project>(created.Id);

        project.ShouldNotBeNull();
        project!.Name.ShouldBe("Updated Name");
        project.Color.ShouldBe("#222222");
        project.Notes.ShouldBe("Updated notes");
    }
}
