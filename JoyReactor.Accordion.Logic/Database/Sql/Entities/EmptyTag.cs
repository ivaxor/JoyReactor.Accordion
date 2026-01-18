using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record EmptyTag : ISqlCreatedAtEntity
{
    public EmptyTag() { }
    public EmptyTag(Api api, int numberId)
    {
        Id = numberId.ToGuid();
        ApiId = api.Id;
        NumberId = numberId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public Guid ApiId { get; set; }
    public virtual Api? Api { get; set; }

    public int NumberId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class EmptyTagEntityTypeConfiguration : IEntityTypeConfiguration<EmptyTag>
{
    public void Configure(EntityTypeBuilder<EmptyTag> builder)
    {
        builder
            .HasOne(e => e.Api)
            .WithMany(e => e.EmptyTags)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.ApiId)
            .OnDelete(DeleteBehavior.Restrict);

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