namespace Tamelo.Api.Domain.Entities;

public class UserProfile : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty; // Supabase user UUID
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public int AutoArchiveDays { get; set; }
}
