using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedPostAttributeEmbeded : ISqlEntity, IParsedPostAttribute
{
    public ParsedPostAttributeEmbeded() { }

    public ParsedPostAttributeEmbeded(PostAttribute attribute, ParsedPost post, IParsedAttributeEmbeded parsedAttribute)
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
    public virtual ParsedYoutube? YouTube { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedPostAttributeEmbededEntityTypeConfiguration : IEntityTypeConfiguration<ParsedPostAttributeEmbeded>
{
    public void Configure(EntityTypeBuilder<ParsedPostAttributeEmbeded> builder)
    {
        builder
            .HasIndex(e => new { e.PostId, e.BandCampId, e.CoubId, e.SoundCloudId, e.VimeoId, e.YouTubeId })
            .IsUnique();

        builder
            .HasOne(e => e.Post)
            .WithMany(e => e.AttributeEmbeds)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(e => e.BandCamp)
            .WithOne(e => e.PostAttributeEmbeded)
            .HasPrincipalKey<ParsedBandCamp>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.BandCampId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasOne(e => e.Coub)
            .WithOne(e => e.PostAttributeEmbeded)
            .HasPrincipalKey<ParsedCoub>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.CoubId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasOne(e => e.SoundCloud)
            .WithOne(e => e.PostAttributeEmbeded)
            .HasPrincipalKey<ParsedSoundCloud>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.SoundCloudId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasOne(e => e.Vimeo)
            .WithOne(e => e.PostAttributeEmbeded)
            .HasPrincipalKey<ParsedVimeo>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.VimeoId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder
            .HasOne(e => e.YouTube)
            .WithOne(e => e.PostAttributeEmbeded)
            .HasPrincipalKey<ParsedYoutube>(e => e.Id)
            .HasForeignKey<ParsedPostAttributeEmbeded>(e => e.YouTubeId)
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