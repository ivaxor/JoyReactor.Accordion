using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedPost : ISqlUpdatedAtEntity
{
    public ParsedPost() { }

    public ParsedPost(Api api, Post post)
    {
        Id = post.NumberId.ToGuid();
        ApiId = api.Id;
        NumberId = post.NumberId;
        ContentVersion = post.ContentVersion.Value;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public Guid ApiId { get; set; }
    public virtual Api? Api { get; set; }

    public int NumberId { get; set; }
    public int ContentVersion { get; set; }

    public virtual ICollection<ParsedPostAttributePicture>? AttributePictures { get; set; }
    public virtual ICollection<ParsedPostAttributeEmbedded>? AttributeEmbeds { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedPostEntityTypeConfiguration : IEntityTypeConfiguration<ParsedPost>
{
    public void Configure(EntityTypeBuilder<ParsedPost> builder)
    {
        builder
            .HasOne(e => e.Api)
            .WithMany(e => e.Posts)
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
            .Property(e => e.ContentVersion)
            .IsRequired(true);

        builder
            .HasMany(e => e.AttributePictures)
            .WithOne(e => e.Post)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.PostId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(e => e.AttributeEmbeds)
            .WithOne(e => e.Post)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.PostId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired(true);
        builder
            .Property(e => e.UpdatedAt)
            .IsRequired(true);
    }
}