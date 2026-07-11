using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.CreatorUserId)
            .IsRequired();

        builder.Property(x => x.AssigneeUserId);

        builder.Property(x => x.Title)
            .HasConversion(
                title => title.Value,
                value => TaskItemTitle.Create(value).Value)
            .HasMaxLength(TaskItemConstants.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                description => description == null ? null : description.Value,
                value => value == null
                    ? null
                    : TaskItemDescription.Create(value).Value);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.DueDate);

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.ArchivedAt);

        builder.HasOne(x => x.Project)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.Creator)
            .WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.Assignee)
            .WithMany(x => x.AssignedTasks)
            .HasForeignKey(x => x.AssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}