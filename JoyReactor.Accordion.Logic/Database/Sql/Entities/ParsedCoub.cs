using JoyReactor.Accordion.Logic.ApiClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedCoub : ISqlEntity, IParsedAttributeEmbeded
{
    public ParsedCoub() { }

    public ParsedCoub(PostAttribute attribute)
    {
        Id = Guid.NewGuid();
        VideoId = attribute.Value;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public string VideoId { get; set; }

    public virtual ParsedPostAttributeEmbeded PostAttributeEmbeded { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedCoubEntityTypeConfiguration : IEntityTypeConfiguration<ParsedCoub>
{
    public void Configure(EntityTypeBuilder<ParsedCoub> builder)
    {
        builder
            .HasIndex(e => e.VideoId)
            .IsUnique();
        builder
            .Property(e => e.VideoId)
            .IsRequired(true);

        builder
            .HasOne(e => e.PostAttributeEmbeded)
            .WithOne(e => e.Coub)
            .HasPrincipalKey<ParsedCoub>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.CoubId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired(true);
        builder
            .Property(e => e.UpdatedAt)
            .IsRequired(true);
    }
}