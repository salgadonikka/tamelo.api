using Tamelo.Api.Application.Projects.Commands.CreateProject;
using Tamelo.Api.Application.Projects.Queries.GetProjects;


namespace Tamelo.Api.Application.FunctionalTests.Projects.Queries;

public class GetProjectsTests : BaseTestFixture
{
    [Test]
    public async Task GetProjects_ReturnsOnlyCurrentUsersProjects()
    {
        // User A creates two projects, then immediately queries while still authenticated as User A
        await RunAsUserAsync("usera@local", "Testing1234!", Array.Empty<string>());

        await SendAsync(new CreateProjectCommand { Name = "User A - Project 1" });
        await SendAsync(new CreateProjectCommand { Name = "User A - Project 2" });

        var userAProjects = await SendAsync(new GetProjectsQuery());

        // User B creates one project, then queries while authenticated as User B
        await RunAsUserAsync("userb@local", "Testing1234!", Array.Empty<string>());

        await SendAsync(new CreateProjectCommand { Name = "User B - Project 1" });

        var userBProjects = await SendAsync(new GetProjectsQuery());

        // User A should see only their 2 projects
        userAProjects.ShouldNotBeNull();
        userAProjects.Count.ShouldBe(2);
        userAProjects.ShouldAllBe(p => p.Name.StartsWith("User A"));

        // User B should see only their 1 project
        userBProjects.ShouldNotBeNull();
        userBProjects.Count.ShouldBe(1);
        userBProjects[0].Name.ShouldBe("User B - Project 1");
    }
}
