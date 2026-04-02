using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamelo.Api.Domain.Entities;
using Tamelo.Api.Domain.Enums;

namespace Tamelo.Api.Infrastructure.Data.Configurations;

public class DayMarkerConfiguration : IEntityTypeConfiguration<DayMarker>
{
    public void Configure(EntityTypeBuilder<DayMarker> builder)
    {
        builder.Property(m => m.Date)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.State)
            .HasConversion(
                s => s.ToString().ToLower(),
                s => Enum.Parse<CircleState>(s, ignoreCase: true))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(m => m.TaskItem)
            .WithMany(t => t.Markers)
            .HasForeignKey(m => m.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.TaskItemId, m.Date }).IsUnique();
    }
}
