using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JoyReactor.Accordion.Logic.Crawlers;

public class TagCrawler(
    ITagClient tagClient,
    SqlDatabaseContext sqlDatabaseContext,
    ILogger<TagCrawler> logger)
    : ITagCrawler
{
    public async Task<ParsedTag?> CrawlAsync(int numberId, CancellationToken cancellationToken)
    {
        var tag = await tagClient.GetAsync(numberId, TagLineType.NEW, cancellationToken);
        if (tag == null)
        {
            logger.LogInformation("No tag found using \"{TagNumberId}\" number id", numberId);
            return null;
        }
        logger.LogInformation("Found \"{TagName}\" tag using {TagNumberId} number id", tag.Name, tag.NumberId);

        await CrawlParentTagAsync(tag, cancellationToken);

        var parsedTag = new ParsedTag(tag);
        await sqlDatabaseContext.ParsedTags.AddIgnoreExistingAsync(parsedTag, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

        return parsedTag;
    }

    public async Task<ParsedTag?> CrawlAsync(string name, CancellationToken cancellationToken)
    {
        var tag = await tagClient.GetByNameAsync(name, TagLineType.NEW, cancellationToken);
        if (tag == null)
        {
            logger.LogInformation("No tag found using \"{TagName}\" name", name);
            return null;
        }
        logger.LogInformation("Found \"{TagName}\" tag using name", tag.Name);

        await CrawlParentTagAsync(tag, cancellationToken);

        var parsedTag = new ParsedTag(tag);
        await sqlDatabaseContext.ParsedTags.AddIgnoreExistingAsync(parsedTag, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

        return parsedTag;
    }

    public async Task CrawlSubTagsAsync(int parentNumberId, CancellationToken cancellationToken)
    {
        var subTags = await tagClient.GetAllSubTagsAsync(parentNumberId, TagLineType.NEW, cancellationToken);
        var parsedSubTags = subTags
            .Select(subTag => new ParsedTag(subTag))
            .ToArray();

        logger.LogInformation("Found {TagsCount} sub tags using {TagNumberId} parent tag number id", parsedSubTags.Count(), parentNumberId);
        if (parsedSubTags.Length == 0)
            return;

        await sqlDatabaseContext.ParsedTags.AddRangeIgnoreExistingAsync(parsedSubTags, cancellationToken);
        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }

    protected async Task CrawlParentTagAsync(Tag tag, CancellationToken cancellationToken)
    {
        var parentTag = tag.Hierarchy
            .Where(t => t.NumberId != tag.NumberId)
            .FirstOrDefault();
        if (parentTag == null)
            return;

        logger.LogInformation("Crawling parent tag for \"{TagName}\" tag", tag.Name);
        await CrawlAsync(parentTag.NumberId, cancellationToken);
    }
}

public interface ITagCrawler
{
    Task<ParsedTag?> CrawlAsync(int numberId, CancellationToken cancellationToken);
    Task<ParsedTag?> CrawlAsync(string name, CancellationToken cancellationToken);
    Task CrawlSubTagsAsync(int parentNumberId, CancellationToken cancellationToken);
}