namespace Tamelo.Api.Application.Tasks.Commands.ImportTasks;

public class ImportTasksCommandValidator : AbstractValidator<ImportTasksCommand>
{
    private static readonly string[] ValidStatuses = ["unplanned", "planned", "ongoing", "completed"];
    private static readonly string[] ValidActions = ["create", "reject"];

    public ImportTasksCommandValidator()
    {
        RuleFor(x => x.UnknownProjectAction)
            .Must(v => ValidActions.Contains(v))
            .WithMessage("UnknownProjectAction must be 'create' or 'reject'.");

        RuleFor(x => x.Rows)
            .NotNull()
            .Must(r => r.Count <= 500)
            .WithMessage("Cannot import more than 500 rows at once.");

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.Title)
                .NotEmpty()
                .MaximumLength(500);

            row.RuleFor(r => r.Description)
                .MaximumLength(2000)
                .When(r => r.Description is not null);

            row.RuleFor(r => r.Project)
                .MaximumLength(200)
                .When(r => r.Project is not null);

            row.RuleFor(r => r.Status)
                .Must(s => ValidStatuses.Contains(s))
                .WithMessage("Status must be one of: unplanned, planned, ongoing, completed.");

            row.RuleFor(r => r.TargetDate)
                .Must(d => DateOnly.TryParse(d, out _))
                .WithMessage("TargetDate must be a valid date (YYYY-MM-DD).")
                .When(r => !string.IsNullOrWhiteSpace(r.TargetDate));
        });
    }
}
