using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;

namespace JoyReactor.Accordion.WebAPI.Models.Responses;

public record CrawlerTaskResponse
{
    public CrawlerTaskResponse() { }
    public CrawlerTaskResponse(CrawlerTask crawlerTask)
    {
        Id = crawlerTask.Id;
        Tag = new ParsedTagThinResponse(crawlerTask.Tag);
        PostLineType = crawlerTask.PostLineType;
        PageCurrent = crawlerTask.PageCurrent;
        PageLast = crawlerTask.PageLast;        
        StartedAt = crawlerTask.StartedAt;
        FinishedAt = crawlerTask.FinishedAt;
        CreatedAt = crawlerTask.CreatedAt;
        UpdatedAt = crawlerTask.UpdatedAt;
    }

    public Guid Id { get; set; }
    public ParsedTagThinResponse Tag { get; set; }

    public PostLineType PostLineType { get; set; }

    public int PageCurrent { get; set; }
    public int? PageLast { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}