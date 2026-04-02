using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamelo.Api.Domain.Entities;

namespace Tamelo.Api.Infrastructure.Data.Configurations;

public class TaskNoteConfiguration : IEntityTypeConfiguration<TaskNote>
{
    public void Configure(EntityTypeBuilder<TaskNote> builder)
    {
        builder.Property(n => n.Content)
            .IsRequired();

        builder.HasOne(n => n.TaskItem)
            .WithMany(t => t.TaskNotes)
            .HasForeignKey(n => n.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.TaskItemId);
    }
}
