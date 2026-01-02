using JoyReactor.Accordion.Logic.ApiClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedSoundCloud : ISqlEntity, IParsedAttributeEmbeded
{
    public ParsedSoundCloud() { }

    public ParsedSoundCloud(PostAttribute attribute)
    {
        Id = Guid.NewGuid();
        UrlPath = attribute.Value;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public string UrlPath { get; set; }

    public virtual ParsedPostAttributeEmbeded PostAttributeEmbeded { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedSoundCloudEntityTypeConfiguration : IEntityTypeConfiguration<ParsedSoundCloud>
{
    public void Configure(EntityTypeBuilder<ParsedSoundCloud> builder)
    {
        builder
            .HasIndex(e => e.UrlPath)
            .IsUnique();
        builder
            .Property(e => e.UrlPath)
            .IsRequired(true);

        builder
            .HasOne(e => e.PostAttributeEmbeded)
            .WithOne(e => e.SoundCloud)
            .HasPrincipalKey<ParsedSoundCloud>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.SoundCloudId)
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