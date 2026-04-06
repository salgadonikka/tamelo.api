using NUnit.Framework;
using Shouldly;
using Tamelo.Api.Application.Projects.Commands.CreateProject;

namespace Tamelo.Api.Application.UnitTests.Projects.Commands;

[TestFixture]
public class CreateProjectCommandValidatorTests
{
    private CreateProjectCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateProjectCommandValidator();
    }

    [Test]
    public async Task Validate_EmptyName_HasValidationErrorOnName()
    {
        var command = new CreateProjectCommand { Name = string.Empty };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Test]
    public async Task Validate_NameExceedingMaxLength_HasValidationErrorOnName()
    {
        var command = new CreateProjectCommand { Name = new string('x', 201) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Test]
    public async Task Validate_ValidCommand_HasNoValidationErrors()
    {
        var command = new CreateProjectCommand { Name = "Valid Project Name" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task Validate_NameAtMaxLength_HasNoValidationErrors()
    {
        var command = new CreateProjectCommand { Name = new string('x', 200) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(command.Name));
    }
}
