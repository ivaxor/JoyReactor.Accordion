using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JoyReactor.Accordion.Workers.BackgroudServices;

public class TopWeekPostsCrawler : BackgroundService, IDisposable
{
    internal readonly IPostClient postClient;
    internal readonly ILogger<TopWeekPostsCrawler> logger;
    internal readonly IServiceScope serviceScope;
    internal readonly SqlDatabaseContext sqlDatabaseContext;

    public TopWeekPostsCrawler(
        IServiceScopeFactory serviceScopeFactory,
        IPostClient postClient,
        ILogger<TopWeekPostsCrawler> logger)
    {
        this.postClient = postClient;
        this.logger = logger;

        serviceScope = serviceScopeFactory.CreateScope();
        sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
    }

    public void Dispose()
    {
        serviceScope.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var startYear = 2009;
        var startWeek = 12;

        startYear = 2018;
        startWeek = 1;

        var endYear = DateTime.UtcNow.Date.Year;
        var endYearWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday) - 1;

        var nsfw = false;

        for (var year = startYear; year <= endYear; year++)
        {
            var yearLastWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(new DateTime(year, 12, 31), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            for (var week = (year == startYear ? startWeek : 0); week <= (year == endYear ? endYearWeek : yearLastWeek); week++)
            {
                logger.LogInformation("Crawling {Week} week of {Year} year top {PostType}", week, year, nsfw ? "nsfw posts" : "posts");
                await CrawlAsync(year, week, nsfw, cancellationToken);
            }
        }
    }

    internal async Task CrawlAsync(int year, int week, bool nsfw, CancellationToken cancellationToken)
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
                var parsedAttributeEmbeded = await CreateAttributeAsync(postAttribute, cancellationToken);
                if (parsedAttributeEmbeded != null)
                    parsedAttributeEmbeds.Add(parsedAttributeEmbeded);

                var parsedPostAttribute = CreatePostAttribute(postAttribute, parsedPost, parsedAttributeEmbeded);
                parsedPostAttributes.Add(parsedPostAttribute);
            }
        }

        await sqlDatabaseContext.ParsedPost.AddRangeIgnoreExistingAsync(parsedPosts, cancellationToken);
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
                ParsedBandCamp => sqlDatabaseContext.ParsedBandCamps.AddRangeIgnoreExistingAsync(group.Cast<ParsedBandCamp>(), cancellationToken),
                ParsedCoub => sqlDatabaseContext.ParsedCoubs.AddRangeIgnoreExistingAsync(group.Cast<ParsedCoub>(), cancellationToken),
                ParsedSoundCloud => sqlDatabaseContext.ParsedSoundClouds.AddRangeIgnoreExistingAsync(group.Cast<ParsedSoundCloud>(), cancellationToken),
                ParsedVimeo => sqlDatabaseContext.ParsedVimeos.AddRangeIgnoreExistingAsync(group.Cast<ParsedVimeo>(), cancellationToken),
                ParsedYoutube => sqlDatabaseContext.ParsedYoutubes.AddRangeIgnoreExistingAsync(group.Cast<ParsedYoutube>(), cancellationToken),
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
                ParsedPostAttributePicture => sqlDatabaseContext.ParsedPostAttributePictures.AddRangeIgnoreExistingAsync(group.Cast<ParsedPostAttributePicture>(), cancellationToken),
                ParsedPostAttributeEmbeded => sqlDatabaseContext.ParsedPostAttributeEmbeds.AddRangeIgnoreExistingAsync(group.Cast<ParsedPostAttributeEmbeded>(), cancellationToken),
                _ => throw new NotImplementedException(),
            });
        }
    }

    internal async Task<IParsedAttributeEmbeded> CreateAttributeAsync(PostAttribute postAttribute, CancellationToken cancellationToken)
    {
        switch (postAttribute.Type)
        {
            case "PICTURE":
                return null;

            case "BANDCAMP":
                var parsedBandCamp = new ParsedBandCamp(postAttribute);
                var existingBandCampId = await sqlDatabaseContext.ParsedBandCamps
                    .Where(bandCamp => bandCamp.AlbumId == parsedBandCamp.AlbumId)
                    .Select(bandCamp => bandCamp.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingBandCampId != default)
                    parsedBandCamp.Id = existingBandCampId;
                return parsedBandCamp;

            case "COUB":
                var parsedCoub = new ParsedCoub(postAttribute);
                var existingCoubId = await sqlDatabaseContext.ParsedCoubs
                    .Where(coub => coub.VideoId == parsedCoub.VideoId)
                    .Select(coub => coub.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingCoubId != default)
                    parsedCoub.Id = existingCoubId;
                return parsedCoub;

            case "SOUNDCLOUD":
                var parsedSoundCloud = new ParsedSoundCloud(postAttribute);
                var existingSoundCloudId = await sqlDatabaseContext.ParsedSoundClouds
                    .Where(soundCloud => soundCloud.UrlPath == soundCloud.UrlPath)
                    .Select(soundCloud => soundCloud.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingSoundCloudId != default)
                    parsedSoundCloud.Id = existingSoundCloudId;
                return parsedSoundCloud;

            case "VIMEO":
                var parsedVimeo = new ParsedVimeo(postAttribute);
                var existingVimeoId = await sqlDatabaseContext.ParsedVimeos
                    .Where(vimeo => vimeo.VideoId == parsedVimeo.VideoId)
                    .Select(vimeo => vimeo.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingVimeoId != default)
                    parsedVimeo.Id = existingVimeoId;
                return parsedVimeo;

            case "YOUTUBE":
                var parsedYouTube = new ParsedYoutube(postAttribute);
                var existingYouTubeId = await sqlDatabaseContext.ParsedYoutubes
                    .Where(youTube => youTube.VideoId == parsedYouTube.VideoId)
                    .Select(youTube => youTube.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingYouTubeId != default)
                    parsedYouTube.Id = existingYouTubeId;
                return parsedYouTube;

            default: throw new NotImplementedException();
        }
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