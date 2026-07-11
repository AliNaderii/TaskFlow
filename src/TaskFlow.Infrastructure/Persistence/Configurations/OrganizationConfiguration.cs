using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => OrganizationName.Create(value).Value)
            .HasMaxLength(OrganizationConstants.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.ArchivedAt);

        builder.HasMany(x => x.Projects)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Navigation(x => x.Projects)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Memberships)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Navigation(x => x.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}