using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedPostAttributePicture
{
    public Guid Id { get; set; }

    public int AttributeId { get; set; }
    public int ImageId { get; set; }

    public Guid PostId { get; set; }
    public virtual ParsedPost Post { get; set; }

    public bool IsVectorCreated { get; set; }
    public DateTime? VectorLastUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
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
            .Property(e => e.ImageId)
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