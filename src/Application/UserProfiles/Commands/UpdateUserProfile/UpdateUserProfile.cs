using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.UserProfiles.Commands.UpdateUserProfile;

[Authorize]
public record UpdateUserProfileCommand : IRequest
{
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public int AutoArchiveDays { get; init; }
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateUserProfileCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == _user.Id, cancellationToken);

        if (profile == null)
        {
            profile = new UserProfile { UserId = _user.Id! };
            _context.UserProfiles.Add(profile);
        }

        profile.Email = request.Email;
        profile.DisplayName = request.DisplayName;
        profile.AvatarUrl = request.AvatarUrl;
        profile.AutoArchiveDays = request.AutoArchiveDays;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
