using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => ProjectName.Create(value).Value)
            .HasMaxLength(ProjectConstants.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                description => description == null ? null : description.Value,
                value => value == null
                    ? null
                    : ProjectDescription.Create(value).Value)
            .HasMaxLength(ProjectConstants.DescriptionMaxLength);

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.ArchivedAt);
    
        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Tasks)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Navigation(x => x.Tasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}