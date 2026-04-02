using Tamelo.Api.Domain.Enums;

namespace Tamelo.Api.Application.DayMarkers.Commands.UpsertDayMarker;

public class UpsertDayMarkerCommandValidator : AbstractValidator<UpsertDayMarkerCommand>
{
    private static readonly string[] ValidStates = Enum.GetNames<CircleState>().Select(s => s.ToLower()).ToArray();

    public UpsertDayMarkerCommandValidator()
    {
        RuleFor(v => v.Date)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Date must be in YYYY-MM-DD format.");

        RuleFor(v => v.State)
            .NotEmpty()
            .Must(s => ValidStates.Contains(s.ToLower()))
            .WithMessage($"State must be one of: {string.Join(", ", ValidStates)}.");
    }
}
