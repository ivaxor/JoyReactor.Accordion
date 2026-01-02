using JoyReactor.Accordion.Logic.ApiClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedBandCamp : ISqlEntity, IParsedAttributeEmbeded
{
    public ParsedBandCamp() { }

    public ParsedBandCamp(PostAttribute attribute)
    {
        Id = Guid.NewGuid();
        AlbumId = attribute.Value;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public string AlbumId { get; set; }

    public virtual ParsedPostAttributeEmbeded PostAttributeEmbeded { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedBandCampEntityTypeConfiguration : IEntityTypeConfiguration<ParsedBandCamp>
{
    public void Configure(EntityTypeBuilder<ParsedBandCamp> builder)
    {
        builder
            .HasIndex(e => e.AlbumId)
            .IsUnique();
        builder
            .Property(e => e.AlbumId)
            .IsRequired(true);

        builder
            .HasOne(e => e.PostAttributeEmbeded)
            .WithOne(e => e.BandCamp)
            .HasPrincipalKey<ParsedBandCamp>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.BandCampId)
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