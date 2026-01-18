using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedTag : ISqlUpdatedAtEntity
{
    public ParsedTag() { }

    public ParsedTag(Api api, Tag tag)
    {
        Id = tag.NumberId.ToGuid();
        ApiId = api.Id;
        MainTagId = tag.NumberId == tag.MainTag.NumberId ? null : tag.MainTag.NumberId.ToGuid();
        ParentId = tag.Hierarchy.Where(t => t.NumberId != tag.NumberId).FirstOrDefault()?.NumberId.ToGuid();
        NumberId = tag.NumberId;
        Name = tag.Name;
        PostCount = tag.PostCount.Value;
        SubscriberCount = tag.SubscriberCount.Value;
        SubTagsCount = tag.Pager.TotalCount.Value;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public Guid ApiId { get; set; }
    public virtual Api? Api { get; set; }

    public Guid? MainTagId { get; set; }
    public virtual ParsedTag? MainTag { get; set; }
    public virtual ICollection<ParsedTag>? Synonyms { get; set; }

    public Guid? ParentId { get; set; }
    public virtual ParsedTag? Parent { get; set; }
    public virtual ICollection<ParsedTag>? SubTags { get; set; }

    public int NumberId { get; set; }
    public string Name { get; set; }
    public int PostCount { get; set; }
    public int SubscriberCount { get; set; }
    public int SubTagsCount { get; set; }

    public virtual ICollection<CrawlerTask>? CrawlerTasks { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedTagEntityTypeConfiguration : IEntityTypeConfiguration<ParsedTag>
{
    public void Configure(EntityTypeBuilder<ParsedTag> builder)
    {
        builder
            .HasOne(e => e.Api)
            .WithMany(e => e.Tags)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.ApiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.MainTag)
            .WithMany(e => e.Synonyms)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.MainTagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.Parent)
            .WithMany(e => e.SubTags)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(e => e.NumberId)
            .IsUnique();
        builder
            .Property(e => e.NumberId)
            .IsRequired(true);

        builder
            .HasIndex(e => e.Name)
            .IsUnique();
        builder
            .Property(e => e.Name)
            .IsRequired(true);

        builder
            .Property(e => e.PostCount)
            .IsRequired(true);

        builder
            .Property(e => e.SubscriberCount)
            .IsRequired(true);

        builder
            .Property(e => e.SubTagsCount)
            .IsRequired(true);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired(true);
        builder
            .Property(e => e.UpdatedAt)
            .IsRequired(true);
    }
}