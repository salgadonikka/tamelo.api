using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.Property(t => t.UserId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(t => t.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => new { t.UserId, t.Archived });
    }
}
