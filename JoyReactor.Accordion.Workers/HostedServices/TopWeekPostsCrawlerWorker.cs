using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JoyReactor.Accordion.Workers.HostedServices;

public class TopWeekPostsCrawlerWorker : IHostedService, IDisposable
{
    internal readonly IPostClient postClient;
    internal readonly ILogger<TopWeekPostsCrawlerWorker> logger;

    internal readonly IServiceScope serviceScope;
    internal readonly SqlDatabaseContext sqlDatabaseContext;
    internal readonly ISqlDatabaseRepository<ParsedPost> parsedPostRepository;
    internal readonly ISqlDatabaseRepository<ParsedPostAttributePicture> parsedPostAttributePictureRepository;
    internal readonly ISqlDatabaseRepository<ParsedPostAttributeEmbeded> parsedPostAttributeEmbededRepository;
    internal readonly ISqlDatabaseRepository<ParsedBandCamp> parsedBandCampRepository;
    internal readonly ISqlDatabaseRepository<ParsedCoub> parsedCoubRepository;
    internal readonly ISqlDatabaseRepository<ParsedSoundCloud> parsedSoundCloudRepository;
    internal readonly ISqlDatabaseRepository<ParsedVimeo> parsedVimeoRepository;
    internal readonly ISqlDatabaseRepository<ParsedYoutube> parsedYoutubeRepository;

    public TopWeekPostsCrawlerWorker(
        IServiceScopeFactory serviceScopeFactory,
        IPostClient postClient,
        ILogger<TopWeekPostsCrawlerWorker> logger)
    {
        this.postClient = postClient;
        this.logger = logger;

        serviceScope = serviceScopeFactory.CreateScope();
        sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
        parsedPostRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedPost>>();
        parsedPostAttributePictureRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedPostAttributePicture>>();
        parsedPostAttributeEmbededRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedPostAttributeEmbeded>>();
        parsedBandCampRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedBandCamp>>();
        parsedCoubRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedCoub>>();
        parsedSoundCloudRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedSoundCloud>>();
        parsedVimeoRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedVimeo>>();
        parsedYoutubeRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlDatabaseRepository<ParsedYoutube>>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var nsfw = false;
        var date = new DateTime(2009, 03, 21);
        var endDate = DateTime.UtcNow.Date - TimeSpan.FromDays(7);

        while (date < endDate)
        {
            var year = date.Year;
            var week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            logger.LogInformation("Crawling {Week} week of {Year} year top {PostType}", week, year, nsfw ? "nsfw posts" : "posts");

            await Crawl(year, week, nsfw, cancellationToken);

            date += TimeSpan.FromDays(7);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        serviceScope.Dispose();
    }

    internal async Task Crawl(int year, int week, bool nsfw, CancellationToken cancellationToken)
    {
        var posts = await postClient.GetWeekTopPostsAsync(year, week, nsfw, cancellationToken);
        logger.LogInformation("Foind {PostCount} top {Week} week of {Year} year {PostType}", posts.Length, week, year, nsfw ? "nsfw posts" : "posts");

        var parsedPosts = new List<ParsedPost>(posts.Length);
        var parsedPostAttributes = new List<IParsedPostAttribute>();
        var parsedAttributeEmbeds = new List<IParsedAttributeEmbeded>();
        foreach (var post in posts)
        {
            var parsedPost = new ParsedPost(post);
            parsedPosts.Add(parsedPost);

            foreach (var postAttribute in post.Attributes)
            {
                var parsedAttributeEmbeded = CreateAttribute(postAttribute);
                if (parsedAttributeEmbeded != null)
                    parsedAttributeEmbeds.Add(parsedAttributeEmbeded);

                var parsedPostAttribute = CreatePostAttribute(postAttribute, parsedPost, parsedAttributeEmbeded);
                parsedPostAttributes.Add(parsedPostAttribute);
            }
        }

        await parsedPostRepository.AddRangeIgnoreExistingAsync(parsedPosts, cancellationToken);
        await AddRangeIgnoreExistingAsync(parsedAttributeEmbeds, cancellationToken);
        await AddRangeIgnoreExistingAsync(parsedPostAttributes, cancellationToken);

        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }

    internal async Task AddRangeIgnoreExistingAsync(IEnumerable<IParsedAttributeEmbeded> parsedAttributeEmbeds, CancellationToken cancellationToken)
    {
        foreach (var group in parsedAttributeEmbeds.GroupBy(attribute => attribute.GetType()))
        {
            await (group.First() switch
            {
                ParsedBandCamp => parsedBandCampRepository.AddRangeIgnoreExistingAsync(group.Cast<ParsedBandCamp>(), cancellationToken),
                ParsedCoub => parsedCoubRepository.AddRangeIgnoreExistingAsync(group.Cast<ParsedCoub>(), cancellationToken),
                ParsedSoundCloud => parsedSoundCloudRepository.AddRangeIgnoreExistingAsync(group.Cast<ParsedSoundCloud>(), cancellationToken),
                ParsedVimeo => parsedVimeoRepository.AddRangeIgnoreExistingAsync(group.Cast<ParsedVimeo>(), cancellationToken),
                ParsedYoutube => parsedYoutubeRepository.AddRangeIgnoreExistingAsync(group.Cast<ParsedYoutube>(), cancellationToken),
                _ => throw new NotImplementedException(),
            });
        }
    }

    internal async Task AddRangeIgnoreExistingAsync(IEnumerable<IParsedPostAttribute> parsedPostAttributes, CancellationToken cancellationToken)
    {
        foreach (var group in parsedPostAttributes.GroupBy(postAttribute => postAttribute.GetType()))
        {
            await (group.First() switch
            {
                ParsedPostAttributePicture => parsedPostAttributePictureRepository.AddRangeIgnoreExistingAsync(group.Cast<ParsedPostAttributePicture>(), cancellationToken),
                ParsedPostAttributeEmbeded => parsedPostAttributeEmbededRepository.AddRangeIgnoreExistingAsync(group.Cast<ParsedPostAttributeEmbeded>(), cancellationToken),
                _ => throw new NotImplementedException(),
            });
        }
    }

    internal static IParsedAttributeEmbeded? CreateAttribute(PostAttribute postAttribute)
    {
        return postAttribute.Type switch
        {
            "PICTURE" => null,
            "BANDCAMP" => new ParsedBandCamp(postAttribute),
            "COUB" => new ParsedCoub(postAttribute),
            "SOUNDCLOUD" => new ParsedSoundCloud(postAttribute),
            "VIMEO" => new ParsedVimeo(postAttribute),
            "YOUTUBE" => new ParsedYoutube(postAttribute),
            _ => throw new NotImplementedException(),
        };
    }

    internal static IParsedPostAttribute CreatePostAttribute(PostAttribute postAttribute, ParsedPost post, IParsedAttributeEmbeded parsedAttribute)
    {
        return postAttribute.Type switch
        {
            "PICTURE" => new ParsedPostAttributePicture(postAttribute, post),
            "BANDCAMP" or "COUB" or "SOUNDCLOUD" or "VIMEO" or "YOUTUBE" => new ParsedPostAttributeEmbeded(postAttribute, post, parsedAttribute),
            _ => throw new NotImplementedException(),
        };
    }
}