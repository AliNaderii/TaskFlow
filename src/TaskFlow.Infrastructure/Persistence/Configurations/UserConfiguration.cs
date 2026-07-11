using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Value)
            .HasMaxLength(EmailConstants.MaxLength)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasConversion(
                displayName => displayName.Value,
                value => DisplayName.Create(value).Value)
            .HasMaxLength(DisplayNameConstants.MaxLength)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.ArchivedAt);

        builder.HasIndex(x => x.Email)
            .IsUnique();
        
        builder.HasMany(x => x.Memberships)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasMany(x => x.CreatedTasks)
            .WithOne(x => x.Creator)
            .HasForeignKey(x => x.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.CreatedTasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasMany(x => x.AssignedTasks)
            .WithOne(x => x.Assignee)
            .HasForeignKey(x => x.AssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.AssignedTasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Author)
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}