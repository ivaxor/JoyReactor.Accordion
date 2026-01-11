using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Database.Vector.Extensions;
using JoyReactor.Accordion.WebAPI.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Qdrant.Client;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("statistics")]
[ApiController]
public class StatisticController(
    SqlDatabaseContext sqlDatabaseContext,
    IQdrantClient qdrantClient,
    IOptions<QdrantSettings> qdrantSettings)
    : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var response = new StatisticsResponse();
        response.Vectors = Convert.ToInt32(await qdrantClient.CountAsync(qdrantSettings.Value.CollectionName, cancellationToken));

        response.ParsedTags = await sqlDatabaseContext.ParsedTags.CountAsync(cancellationToken);
        response.EmptyTags = await sqlDatabaseContext.EmptyTags.CountAsync(cancellationToken);
        response.ParsedPosts = await sqlDatabaseContext.ParsedPosts.CountAsync(cancellationToken);

        response.ParsedPostAttributePictures = await sqlDatabaseContext.ParsedPostAttributePictures.CountAsync(cancellationToken);
        response.ParsedPostAttributePicturesWithVector = await sqlDatabaseContext.ParsedPostAttributePictures
            .Where(postAttribute => postAttribute.IsVectorCreated == true)
            .CountAsync(cancellationToken);
        response.ParsedPostAttributePicturesWithoutVector = await sqlDatabaseContext.ParsedPostAttributePictures
            .Where(postAttribute => postAttribute.IsVectorCreated == false)
            .CountAsync(cancellationToken);

        response.ParsedPostAttributeEmbeds = await sqlDatabaseContext.ParsedPostAttributeEmbeds.CountAsync(cancellationToken);

        response.ParsedBandCamps = await sqlDatabaseContext.ParsedBandCamps.CountAsync(cancellationToken);
        response.ParsedCoubs = await sqlDatabaseContext.ParsedCoubs.CountAsync(cancellationToken);
        response.ParsedSoundClouds = await sqlDatabaseContext.ParsedSoundClouds.CountAsync(cancellationToken);
        response.ParsedVimeos = await sqlDatabaseContext.ParsedVimeos.CountAsync(cancellationToken);
        response.ParsedYouTubes = await sqlDatabaseContext.ParsedYouTubes.CountAsync(cancellationToken);

        return Ok(response);
    }
}