namespace Tamelo.Api.Application.UserProfiles.Queries.GetUserProfile;

public record UserProfileDto(
    int Id,
    string UserId,
    string? Email,
    string? DisplayName,
    string? AvatarUrl,
    int AutoArchiveDays);
