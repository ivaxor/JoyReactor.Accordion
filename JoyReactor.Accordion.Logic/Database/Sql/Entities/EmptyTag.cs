using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record EmptyTag : ISqlEntity
{
    public EmptyTag() { }
    public EmptyTag(int numberId)
    {
        Id = numberId.ToGuid();
        NumberId = numberId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public int NumberId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class EmptyTagEntityTypeConfiguration : IEntityTypeConfiguration<EmptyTag>
{
    public void Configure(EntityTypeBuilder<EmptyTag> builder)
    {
        builder
            .HasIndex(e => e.NumberId)
            .IsUnique();
        builder
            .Property(e => e.NumberId)
            .IsRequired(true);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired(true);
    }
}