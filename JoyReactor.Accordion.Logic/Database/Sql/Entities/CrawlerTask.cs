namespace JoyReactor.Accordion.Logic.Database.Sql.Entities;

public record CrawlerTask : ISqlEntity
{
    public Guid Id { get; set; }

    public int Tag { get; set; }

    public bool Nsfw { get; set; }
    public bool ExcludeSfw { get; set; }
    public bool ExcludeNsfw { get; set; }

    public PostLineType PostLineType { get; set; }
    public int? PageFrom { get; set; }
    public int? PageTo { get; set; }
    public int? PageCurrent { get; set; }

    public DateTime? DateTimeFromUtc { get; set; }
    public DateTime? DateTimeToUtc { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum PostLineType
{
    All,
    New,
    Good,
    Best,
}