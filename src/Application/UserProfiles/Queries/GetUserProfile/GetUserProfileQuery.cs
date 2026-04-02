using Tamelo.Api.Application.Common.Interfaces;
using Tamelo.Api.Application.Common.Security;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Application.UserProfiles.Queries.GetUserProfile;

[Authorize]
public record GetUserProfileQuery : IRequest<UserProfileDto>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetUserProfileQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == _user.Id, cancellationToken);

        if (profile == null)
        {
            profile = new UserProfile { UserId = _user.Id! };
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new UserProfileDto(profile.Id, profile.UserId, profile.Email, profile.DisplayName, profile.AvatarUrl, profile.AutoArchiveDays);
    }
}
