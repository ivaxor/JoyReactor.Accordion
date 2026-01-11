using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Sql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JoyReactor.Accordion.Logic.Parsers;

public class PostParser(
    SqlDatabaseContext sqlDatabaseContext,
    ILogger<PostParser> logger)
    : IPostParser
{
    public Task ParseAsync(Post post, CancellationToken cancellationToken)
    {
        return ParseAsync([post], cancellationToken);
    }

    public async Task ParseAsync(IEnumerable<Post> posts, CancellationToken cancellationToken)
    {
        if (posts.Count() == 0)
            return;

        await using var transaction = await sqlDatabaseContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var postNumberIds = posts.Select(p => p.NumberId).ToArray();
            var existingPostContentVersions = await sqlDatabaseContext.ParsedPosts
                .AsNoTracking()
                .Where(post => postNumberIds.Contains(post.NumberId))
                .ToDictionaryAsync(post => post.NumberId, post => post.ContentVersion, cancellationToken);
            foreach (var post in posts)
            {
                if (existingPostContentVersions.TryGetValue(post.NumberId, out var contentVersion) && post.ContentVersion != contentVersion)
                {
                    logger.LogInformation("Post {PostNubmerId} content version changed. Deleting old post information.", post.NumberId);
                    await sqlDatabaseContext.ParsedPosts.Where(p => p.NumberId == post.NumberId).ExecuteDeleteAsync(cancellationToken);
                }
            }
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

            var parsedPosts = new List<ParsedPost>(posts.Count());
            var parsedPostAttributes = new List<IParsedPostAttribute>();
            var parsedAttributeEmbeds = new List<IParsedAttributeEmbedded>();
            foreach (var post in posts)
            {
                if (existingPostContentVersions.TryGetValue(post.NumberId, out var contentVersion) && post.ContentVersion == contentVersion)
                {
                    logger.LogInformation("Post {PostNumberId} content version change didn't changed. Skipping post.", post.NumberId);
                    continue;
                }

                var parsedPost = new ParsedPost(post);
                parsedPosts.Add(parsedPost);

                foreach (var postAttribute in post.Attributes)
                {
                    var parsedAttributeEmbedded = await CreateAttributeAsync(postAttribute, cancellationToken);
                    if (parsedAttributeEmbedded != null)
                    {
                        parsedAttributeEmbedded = TryToGetExistring(parsedAttributeEmbeds, parsedAttributeEmbedded);
                        parsedAttributeEmbeds.Add(parsedAttributeEmbedded);
                    }

                    var parsedPostAttribute = CreatePostAttribute(postAttribute, parsedPost, parsedAttributeEmbedded);
                    parsedPostAttributes.Add(parsedPostAttribute);
                }
            }

            await sqlDatabaseContext.ParsedPosts.AddRangeAsync(parsedPosts, cancellationToken);
            await AddRangeAsync(parsedPostAttributes, cancellationToken);
            await UpsertRangeAsync(parsedAttributeEmbeds, cancellationToken);

            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    protected async Task UpsertRangeAsync(IEnumerable<IParsedAttributeEmbedded> parsedAttributeEmbeds, CancellationToken cancellationToken)
    {
        foreach (var group in parsedAttributeEmbeds.GroupBy(attribute => attribute.GetType()))
        {
            await (group.First() switch
            {
                ParsedBandCamp => sqlDatabaseContext.ParsedBandCamps.UpsertRangeAsync(group.Cast<ParsedBandCamp>(), cancellationToken),
                ParsedCoub => sqlDatabaseContext.ParsedCoubs.UpsertRangeAsync(group.Cast<ParsedCoub>(), cancellationToken),
                ParsedSoundCloud => sqlDatabaseContext.ParsedSoundClouds.UpsertRangeAsync(group.Cast<ParsedSoundCloud>(), cancellationToken),
                ParsedVimeo => sqlDatabaseContext.ParsedVimeos.UpsertRangeAsync(group.Cast<ParsedVimeo>(), cancellationToken),
                ParsedYouTube => sqlDatabaseContext.ParsedYouTubes.UpsertRangeAsync(group.Cast<ParsedYouTube>(), cancellationToken),
                _ => throw new NotImplementedException(),
            });
        }
    }

    protected async Task AddRangeAsync(IEnumerable<IParsedPostAttribute> parsedPostAttributes, CancellationToken cancellationToken)
    {
        foreach (var group in parsedPostAttributes.GroupBy(postAttribute => postAttribute.GetType()))
        {
            await (group.First() switch
            {
                ParsedPostAttributePicture => sqlDatabaseContext.ParsedPostAttributePictures.AddRangeAsync(group.Cast<ParsedPostAttributePicture>(), cancellationToken),
                ParsedPostAttributeEmbedded => sqlDatabaseContext.ParsedPostAttributeEmbeds.AddRangeAsync(group.Cast<ParsedPostAttributeEmbedded>(), cancellationToken),
                _ => throw new NotImplementedException(),
            });
        }
    }

    protected async Task<IParsedAttributeEmbedded> CreateAttributeAsync(PostAttribute postAttribute, CancellationToken cancellationToken)
    {
        switch (postAttribute.Type)
        {
            case "PICTURE":
                return null;

            case "BANDCAMP":
                var parsedBandCamp = new ParsedBandCamp(postAttribute);
                var existingBandCampId = await sqlDatabaseContext.ParsedBandCamps
                    .Where(bandCamp => bandCamp.UrlPath == parsedBandCamp.UrlPath)
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
                    .Where(soundCloud => soundCloud.UrlPath == parsedSoundCloud.UrlPath)
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
                var parsedYouTube = new ParsedYouTube(postAttribute);
                var existingYouTubeId = await sqlDatabaseContext.ParsedYouTubes
                    .Where(youTube => youTube.VideoId == parsedYouTube.VideoId)
                    .Select(youTube => youTube.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingYouTubeId != default)
                    parsedYouTube.Id = existingYouTubeId;
                return parsedYouTube;

            default:
                throw new NotImplementedException();
        }
    }

    protected static IParsedAttributeEmbedded TryToGetExistring(IEnumerable<IParsedAttributeEmbedded> parsedAttributes, IParsedAttributeEmbedded parsedAttribute)
    {
        var filteredParsedAttributes = parsedAttributes.Where(pa => pa.GetType() == parsedAttribute.GetType());
        if (!filteredParsedAttributes.Any())
            return parsedAttribute;

        return parsedAttribute switch
        {
            ParsedBandCamp parsedBandCamp => filteredParsedAttributes.Cast<ParsedBandCamp>().FirstOrDefault(pa => pa.UrlPath == parsedBandCamp.UrlPath, parsedBandCamp),
            ParsedCoub parsedCoub => filteredParsedAttributes.Cast<ParsedCoub>().FirstOrDefault(pa => pa.VideoId == parsedCoub.VideoId, parsedCoub),
            ParsedSoundCloud parsedSoundCloud => filteredParsedAttributes.Cast<ParsedSoundCloud>().FirstOrDefault(pa => pa.UrlPath == parsedSoundCloud.UrlPath, parsedSoundCloud),
            ParsedVimeo parsedVimeo => filteredParsedAttributes.Cast<ParsedVimeo>().FirstOrDefault(pa => pa.VideoId == parsedVimeo.VideoId, parsedVimeo),
            ParsedYouTube parsedYouTube => filteredParsedAttributes.Cast<ParsedYouTube>().FirstOrDefault(pa => pa.VideoId == parsedYouTube.VideoId, parsedYouTube),
            _ => throw new NotImplementedException(),
        };
    }

    protected static IParsedPostAttribute CreatePostAttribute(PostAttribute postAttribute, ParsedPost post, IParsedAttributeEmbedded parsedAttribute)
    {
        return postAttribute.Type switch
        {
            "PICTURE" => new ParsedPostAttributePicture(postAttribute, post),
            "BANDCAMP" or "COUB" or "SOUNDCLOUD" or "VIMEO" or "YOUTUBE" => new ParsedPostAttributeEmbedded(postAttribute, post, parsedAttribute),
            _ => throw new NotImplementedException(),
        };
    }
}

public interface IPostParser
{
    Task ParseAsync(Post post, CancellationToken cancellationToken);
    Task ParseAsync(IEnumerable<Post> posts, CancellationToken cancellationToken);
}