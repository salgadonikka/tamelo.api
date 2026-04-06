using NUnit.Framework;
using Shouldly;
using Tamelo.Api.Application.Projects.Commands.UpdateProject;

namespace Tamelo.Api.Application.UnitTests.Projects.Commands;

[TestFixture]
public class UpdateProjectCommandValidatorTests
{
    private UpdateProjectCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateProjectCommandValidator();
    }

    [Test]
    public async Task Validate_EmptyName_HasValidationErrorOnName()
    {
        var command = new UpdateProjectCommand { Id = 1, Name = string.Empty };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Test]
    public async Task Validate_NameExceedingMaxLength_HasValidationErrorOnName()
    {
        var command = new UpdateProjectCommand { Id = 1, Name = new string('x', 201) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Test]
    public async Task Validate_ValidCommand_HasNoValidationErrors()
    {
        var command = new UpdateProjectCommand { Id = 1, Name = "Updated Project Name" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task Validate_NameAtMaxLength_HasNoValidationErrors()
    {
        var command = new UpdateProjectCommand { Id = 1, Name = new string('x', 200) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(command.Name));
    }
}
