using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.Property(p => p.UserId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(p => p.Email)
            .HasMaxLength(256);

        builder.Property(p => p.DisplayName)
            .HasMaxLength(200);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(2048);

        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
