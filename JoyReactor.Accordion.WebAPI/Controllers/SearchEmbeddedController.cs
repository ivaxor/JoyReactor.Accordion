using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.WebAPI.Models.Requests;
using JoyReactor.Accordion.WebAPI.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("api/search/embedded")]
[ApiController]
public class SearchEmbeddedController(SqlDatabaseContext sqlDatabaseContext)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SearchAsync([FromBody] SearchEmbededRequest request, CancellationToken cancellationToken)
    {
        var entityId = request.Type switch
        {
            SearchEmbeddedType.BandCamp => (await sqlDatabaseContext.ParsedBandCamps.FirstOrDefaultAsync(bandCamp => bandCamp.UrlPath == request.Text, cancellationToken))?.Id,
            SearchEmbeddedType.Coub => (await sqlDatabaseContext.ParsedCoubs.FirstOrDefaultAsync(coub => coub.VideoId == request.Text, cancellationToken))?.Id,
            SearchEmbeddedType.SoundCloud => (await sqlDatabaseContext.ParsedSoundClouds.FirstOrDefaultAsync(soundCloud => soundCloud.UrlPath == request.Text, cancellationToken))?.Id,            
            SearchEmbeddedType.Vimeo => (await sqlDatabaseContext.ParsedVimeos.FirstOrDefaultAsync(vimeo => vimeo.VideoId == request.Text, cancellationToken))?.Id,
            SearchEmbeddedType.YouTube => (await sqlDatabaseContext.ParsedYoutubes.FirstOrDefaultAsync(youTube => youTube.VideoId == request.Text, cancellationToken))?.Id,
            _ => throw new NotImplementedException()
        };
        if (entityId == null)
            return NotFound();

        var query = sqlDatabaseContext.ParsedPostAttributeEmbeds.AsQueryable();
        query = request.Type switch
        {
            SearchEmbeddedType.BandCamp => query.Where(x => x.BandCampId == entityId),
            SearchEmbeddedType.Coub => query.Where(coub => coub.CoubId == entityId),
            SearchEmbeddedType.SoundCloud => query.Where(soundCloud => soundCloud.SoundCloudId == entityId),
            SearchEmbeddedType.Vimeo => query.Where(vimeo => vimeo.VimeoId == entityId),
            SearchEmbeddedType.YouTube => query.Where(youTube => youTube.YouTubeId == entityId),
            _ => query
        };

        var postIds = await query.Select(postAttribute => postAttribute.PostId).ToArrayAsync(cancellationToken);

        return Ok(new SearchEmbeddedResponse(postIds));
    }
}