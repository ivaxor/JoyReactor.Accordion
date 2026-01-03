using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedPostAttributePicture : ISqlEntity, IParsedPostAttribute
{
    public ParsedPostAttributePicture() { }

    public ParsedPostAttributePicture(PostAttribute attribute, ParsedPost post)
    {
        Id = attribute.NumberId.ToGuid();
        AttributeId = attribute.NumberId;
        ImageType = attribute.Image.Type switch
        {
            "PNG" => ParsedPostAttributePictureType.PNG,
            "JPEG" => ParsedPostAttributePictureType.JPEG,
            "GIF" => ParsedPostAttributePictureType.GIF,
            "BMP" => ParsedPostAttributePictureType.BMP,
            "TIFF" => ParsedPostAttributePictureType.TIFF,
            "MP4" => ParsedPostAttributePictureType.MP4,
            "WEBM" => ParsedPostAttributePictureType.WEBM,
            _ => throw new NotImplementedException(),
        };
        PostId = post.Id;
        IsVectorCreated = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public int AttributeId { get; set; }
    public ParsedPostAttributePictureType ImageType { get; set; }

    public Guid PostId { get; set; }
    public virtual ParsedPost Post { get; set; }

    public bool IsVectorCreated { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum ParsedPostAttributePictureType
{
    PNG,
    JPEG,
    GIF,
    BMP,
    TIFF,
    MP4,
    WEBM,
}

public class ParsedPostAttributePictureEntityTypeConfiguration : IEntityTypeConfiguration<ParsedPostAttributePicture>
{
    public void Configure(EntityTypeBuilder<ParsedPostAttributePicture> builder)
    {
        builder
            .HasIndex(e => new { e.PostId, e.AttributeId })
            .IsUnique();
        builder
            .Property(e => e.AttributeId)
            .IsRequired(true);

        builder
            .HasOne(e => e.Post)
            .WithMany(e => e.AttributePictures)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(e => e.IsVectorCreated);
        builder
            .Property(e => e.IsVectorCreated)
            .IsRequired(true);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired(true);
        builder
            .Property(e => e.UpdatedAt)
            .IsRequired(true);
    }
}