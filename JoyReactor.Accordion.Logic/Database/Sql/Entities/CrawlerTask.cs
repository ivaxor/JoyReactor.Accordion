using JoyReactor.Accordion.Logic.ApiClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record CrawlerTask : ISqlUpdatedAtEntity
{
    public Guid Id { get; set; }

    public Guid TagId { get; set; }
    public virtual ParsedTag? Tag { get; set; }

    public PostLineType PostLineType { get; set; }
    public int PageCurrent { get; set; }
    public int? PageLast { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CrawlerTaskEntityTypeConfiguration : IEntityTypeConfiguration<CrawlerTask>
{
    public void Configure(EntityTypeBuilder<CrawlerTask> builder)
    {
        builder
            .HasOne(e => e.Tag)
            .WithMany(e => e.CrawlerTasks)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(e => e.PostLineType)
            .IsRequired(true);

        builder
            .Property(e => e.PageCurrent)
            .HasDefaultValue(1)
            .IsRequired(true);

        builder
            .Property(e => e.PageLast)
            .IsRequired(false);

        builder
           .Property(e => e.StartedAt)
           .IsRequired(false);

        builder
           .Property(e => e.FinishedAt)
           .IsRequired(false);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired(true);

        builder
            .Property(e => e.UpdatedAt)
            .IsRequired(true);
    }
}