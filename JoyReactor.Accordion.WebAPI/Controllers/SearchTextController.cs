using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Extensions;
using JoyReactor.Accordion.WebAPI.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("api/search/text")]
[ApiController]
public class SearchTextController(SqlDatabaseContext sqlDatabaseContext)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SearchAsync([FromBody] SearchTextRequest request, CancellationToken cancellationToken)
    {
        var postIds = (Guid[])null;

        switch (request.Type)
        {
            case SearchTextType.BandCamp:
                var parsedBandCamp = await sqlDatabaseContext.ParsedBandCamps.FirstOrDefaultAsync(bandCamp => bandCamp.UrlPath == request.Text, cancellationToken);
                if (parsedBandCamp == null)
                    return NotFound();

                postIds = await sqlDatabaseContext.ParsedPostAttributeEmbeds
                    .Where(postAttribute => postAttribute.BandCampId == parsedBandCamp.Id)
                    .Select(postAttribute => postAttribute.PostId)
                    .ToArrayAsync(cancellationToken);
                break;

            case SearchTextType.Coub:
                var parsedCoub = await sqlDatabaseContext.ParsedCoubs.FirstOrDefaultAsync(coub => coub.VideoId == request.Text, cancellationToken);
                if (parsedCoub == null)
                    return NotFound();

                postIds = await sqlDatabaseContext.ParsedPostAttributeEmbeds
                    .Where(postAttribute => postAttribute.CoubId == parsedCoub.Id)
                    .Select(postAttribute => postAttribute.PostId)
                    .ToArrayAsync(cancellationToken);
                break;

            case SearchTextType.SoundCloud:
                var parsedSoundCloud = await sqlDatabaseContext.ParsedSoundClouds.FirstOrDefaultAsync(soundCloud => soundCloud.UrlPath == request.Text, cancellationToken);
                if (parsedSoundCloud == null)
                    return NotFound();

                postIds = await sqlDatabaseContext.ParsedPostAttributeEmbeds
                    .Where(postAttribute => postAttribute.SoundCloudId == parsedSoundCloud.Id)
                    .Select(postAttribute => postAttribute.PostId)
                    .ToArrayAsync(cancellationToken);
                break;

            case SearchTextType.Vimeo:
                var parsedVimeo = await sqlDatabaseContext.ParsedVimeos.FirstOrDefaultAsync(vimeo => vimeo.VideoId == request.Text, cancellationToken);
                if (parsedVimeo == null)
                    return NotFound();

                postIds = await sqlDatabaseContext.ParsedPostAttributeEmbeds
                    .Where(postAttribute => postAttribute.VimeoId == parsedVimeo.Id)
                    .Select(postAttribute => postAttribute.PostId)
                    .ToArrayAsync(cancellationToken);
                break;

            case SearchTextType.YouTube:
                var parsedYouTube = await sqlDatabaseContext.ParsedYoutubes.FirstOrDefaultAsync(youTube => youTube.VideoId == request.Text, cancellationToken);
                if (parsedYouTube == null)
                    return NotFound();

                postIds = await sqlDatabaseContext.ParsedPostAttributeEmbeds
                    .Where(postAttribute => postAttribute.YouTubeId == parsedYouTube.Id)
                    .Select(postAttribute => postAttribute.PostId)
                    .ToArrayAsync(cancellationToken);
                break;

            default:
                return BadRequest();
        }

        var postNumberIds = postIds
            .Select(id => id.ToInt())
            .ToArray();

        return Ok(postNumberIds);
    }
}