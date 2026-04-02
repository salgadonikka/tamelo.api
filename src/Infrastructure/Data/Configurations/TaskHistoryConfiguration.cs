using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Infrastructure.Data.Configurations;

public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.Property(h => h.UserId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(h => h.EventType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.FieldName)
            .HasMaxLength(100);

        builder.HasOne(h => h.TaskItem)
            .WithMany()
            .HasForeignKey(h => h.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => new { h.TaskItemId, h.CreatedAt });
    }
}
