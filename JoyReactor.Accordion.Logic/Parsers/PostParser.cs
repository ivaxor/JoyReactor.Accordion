using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.Logic.Parsers;

public class PostParser(SqlDatabaseContext sqlDatabaseContext) : IPostParser
{
    public async Task ParseAsync(Post post, CancellationToken cancellationToken)
    {
        var parsedPostAttributes = new List<IParsedPostAttribute>();
        var parsedAttributeEmbeds = new List<IParsedAttributeEmbeded>();

        var parsedPost = new ParsedPost(post);
        foreach (var postAttribute in post.Attributes)
        {
            var parsedAttributeEmbeded = await CreateAttributeAsync(postAttribute, cancellationToken);
            if (parsedAttributeEmbeded != null)
                parsedAttributeEmbeds.Add(parsedAttributeEmbeded);

            var parsedPostAttribute = CreatePostAttribute(postAttribute, parsedPost, parsedAttributeEmbeded);
            parsedPostAttributes.Add(parsedPostAttribute);
        }

        await sqlDatabaseContext.ParsedPost.AddIgnoreExistingAsync(parsedPost, cancellationToken);
        await AddRangeIgnoreExistingAsync(parsedAttributeEmbeds, cancellationToken);
        await AddRangeIgnoreExistingAsync(parsedPostAttributes, cancellationToken);

        await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ParseAsync(IEnumerable<Post> posts, CancellationToken cancellationToken)
    {
        var parsedPosts = new List<ParsedPost>(posts.Count());
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

            default:
                throw new NotImplementedException();
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

public interface IPostParser
{
    Task ParseAsync(Post post, CancellationToken cancellationToken);
    Task ParseAsync(IEnumerable<Post> posts, CancellationToken cancellationToken);
}