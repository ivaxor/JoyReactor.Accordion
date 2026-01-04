using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record ParsedTag : ISqlEntity
{
    public ParsedTag() { }

    public ParsedTag(Tag tag)
    {
        Id = tag.NumberId.ToGuid();
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

    public Guid? MainTagId { get; set; }
    public virtual ParsedTag? MainTag { get; set; }
    public virtual IEnumerable<ParsedTag>? Synonyms { get; set; }

    public Guid? ParentId { get; set; }
    public virtual ParsedTag? Parent { get; set; }
    public virtual IEnumerable<ParsedTag>? SubTags { get; set; }

    public int NumberId { get; set; }
    public string Name { get; set; }
    public int PostCount { get; set; }
    public int SubscriberCount { get; set; }
    public int SubTagsCount { get; set; }

    public virtual IEnumerable<CrawlerTask>? CrawlerTasks { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParsedTagEntityTypeConfiguration : IEntityTypeConfiguration<ParsedTag>
{
    public void Configure(EntityTypeBuilder<ParsedTag> builder)
    {
        builder
            .HasOne(e => e.MainTag)
            .WithMany(e => e.Synonyms)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.MainTagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(e => e.Parent)
            .WithMany(e => e.SubTags)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

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