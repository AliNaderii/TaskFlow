using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TaskId)
            .IsRequired();

        builder.Property(x => x.AuthorUserId)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasConversion(
                content => content.Value,
                value => CommentContent.Create(value).Value)
            .HasMaxLength(CommentConstants.ContentMaxLength)
            .IsRequired();

        builder.Property(x => x.IsEdited)
            .IsRequired();

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.ArchivedAt);
    }
}