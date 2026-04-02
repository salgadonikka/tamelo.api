namespace Tamelo.Api.Application.TaskNotes.Commands.CreateTaskNote;

public class CreateTaskNoteCommandValidator : AbstractValidator<CreateTaskNoteCommand>
{
    public CreateTaskNoteCommandValidator()
    {
        RuleFor(v => v.Content)
            .NotEmpty();
    }
}
