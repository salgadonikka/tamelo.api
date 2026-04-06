using Tamelo.Api.Application.Common.Exceptions;
using Tamelo.Api.Application.Projects.Commands.CreateProject;
using Tamelo.Api.Domain.Entities;


namespace Tamelo.Api.Application.FunctionalTests.Projects.Commands;

public class CreateProjectTests : BaseTestFixture
{
    [Test]
    public async Task CreateProject_WithoutTitle_ThrowsValidationException()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateProjectCommand { Name = string.Empty };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task CreateProject_WithValidData_CreatesProject()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateProjectCommand
        {
            Name = "My Test Project",
            Color = "#FF5733",
            Notes = "Some notes"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(0);

        var project = await FindAsync<Project>(result.Id);

        project.ShouldNotBeNull();
        project!.Name.ShouldBe(command.Name);
        project.Color.ShouldBe(command.Color);
        project.Notes.ShouldBe(command.Notes);
        project.UserId.ShouldBe(userId);
    }
}
