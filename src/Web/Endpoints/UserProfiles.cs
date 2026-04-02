using Microsoft.AspNetCore.Http.HttpResults;
using Tamelo.Api.Application.UserProfiles.Commands.UpdateUserProfile;
using Tamelo.Api.Application.UserProfiles.Queries.GetUserProfile;

namespace Tamelo.Api.Web.Endpoints;

public class UserProfiles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetUserProfile, "me").RequireAuthorization();
        groupBuilder.MapPut(UpdateUserProfile, "me").RequireAuthorization();
    }

    [EndpointName(nameof(GetUserProfile))]
    [EndpointSummary("Get the current user's profile")]
    [EndpointDescription("Returns the authenticated user's profile. Creates one with default values if it does not exist yet.")]
    public async Task<Ok<UserProfileDto>> GetUserProfile(ISender sender)
    {
        var result = await sender.Send(new GetUserProfileQuery());
        return TypedResults.Ok(result);
    }

    [EndpointName(nameof(UpdateUserProfile))]
    [EndpointSummary("Update the current user's profile")]
    public async Task<NoContent> UpdateUserProfile(ISender sender, UpdateUserProfileCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }
}
