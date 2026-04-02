namespace Tamelo.Api.Application.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(v => v.AutoArchiveDays)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.AvatarUrl)
            .MaximumLength(2048)
            .When(v => v.AvatarUrl != null);

        RuleFor(v => v.DisplayName)
            .MaximumLength(200)
            .When(v => v.DisplayName != null);
    }
}
