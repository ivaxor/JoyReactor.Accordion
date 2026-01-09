using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedPostAttributeEmbedded : ISqlUpdatedAtEntity, IParsedPostAttribute
{
    public ParsedPostAttributeEmbedded() { }

    public ParsedPostAttributeEmbedded(PostAttribute attribute, ParsedPost post, IParsedAttributeEmbedded parsedAttribute)
    {
        Id = attribute.NumberId.ToGuid();
        PostId = post.Id;

        switch (attribute.Type)
        {
            case "BANDCAMP":
                BandCampId = parsedAttribute.Id;
                break;
            case "COUB":
                CoubId = parsedAttribute.Id;
                break;
            case "SOUNDCLOUD":
                SoundCloudId = parsedAttribute.Id;
                break;
            case "VIMEO":
                VimeoId = parsedAttribute.Id;
                break;
            case "YOUTUBE":
                YouTubeId = parsedAttribute.Id;
                break;
            default:
                throw new NotImplementedException();
        }

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public Guid PostId { get; set; }
    public virtual ParsedPost Post { get; set; }

    public Guid? BandCampId { get; set; }
    public virtual ParsedBandCamp? BandCamp { get; set; }

    public Guid? CoubId { get; set; }
    public virtual ParsedCoub? Coub { get; set; }

    public Guid? SoundCloudId { get; set; }
    public virtual ParsedSoundCloud? SoundCloud { get; set; }

    public Guid? VimeoId { get; set; }
    public virtual ParsedVimeo? Vimeo { get; set; }

    public Guid? YouTubeId { get; set; }
    public virtual ParsedYouTube? YouTube { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedPostAttributeEmbeddedEntityTypeConfiguration : IEntityTypeConfiguration<ParsedPostAttributeEmbedded>
{
    public void Configure(EntityTypeBuilder<ParsedPostAttributeEmbedded> builder)
    {
        builder
            .HasOne(e => e.Post)
            .WithMany(e => e.AttributeEmbeds)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(e => e.BandCampId)
            .IsUnique(false);
        builder
            .HasOne(e => e.BandCamp)
            .WithOne(e => e.PostAttributeEmbedded)
            .HasPrincipalKey<ParsedBandCamp>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbedded>(e => e.BandCampId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasIndex(e => e.CoubId)
            .IsUnique(false);
        builder
            .HasOne(e => e.Coub)
            .WithOne(e => e.PostAttributeEmbedded)
            .HasPrincipalKey<ParsedCoub>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbedded>(e => e.CoubId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasIndex(e => e.SoundCloudId)
            .IsUnique(false);
        builder
            .HasOne(e => e.SoundCloud)
            .WithOne(e => e.PostAttributeEmbedded)
            .HasPrincipalKey<ParsedSoundCloud>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbedded>(e => e.SoundCloudId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasIndex(e => e.VimeoId)
            .IsUnique(false);
        builder
            .HasOne(e => e.Vimeo)
            .WithOne(e => e.PostAttributeEmbedded)
            .HasPrincipalKey<ParsedVimeo>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbedded>(e => e.VimeoId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasIndex(e => e.YouTubeId)
            .IsUnique(false);
        builder
            .HasOne(e => e.YouTube)
            .WithOne(e => e.PostAttributeEmbedded)
            .HasPrincipalKey<ParsedYouTube>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbedded>(e => e.YouTubeId)
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