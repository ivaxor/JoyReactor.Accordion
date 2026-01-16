using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Database.Vector.Extensions;
using JoyReactor.Accordion.WebAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Qdrant.Client;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("statistics")]
[ApiController]
public class StatisticController(
    IMemoryCache memoryCache,
    SqlDatabaseContext sqlDatabaseContext,
    IQdrantClient qdrantClient,
    IOptions<QdrantSettings> qdrantSettings)
    : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{nameof(StatisticController)}.{nameof(GetAsync)}";
        var response = await memoryCache.GetOrCreateAsync(cacheKey, cacheEntry =>
        {
            cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(1);
            return GetStatisticsAsync(cancellationToken);
        });

        return Ok(response);
    }

    protected async Task<StatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var statitics = new StatisticsResponse();
        statitics.Vectors = Convert.ToInt32(await qdrantClient.CountAsync(qdrantSettings.Value.CollectionName, cancellationToken));

        statitics.ParsedTags = await sqlDatabaseContext.ParsedTags.CountAsync(cancellationToken);
        statitics.EmptyTags = await sqlDatabaseContext.EmptyTags.CountAsync(cancellationToken);
        statitics.ParsedPosts = await sqlDatabaseContext.ParsedPosts.CountAsync(cancellationToken);

        statitics.ParsedPostAttributePictures = await sqlDatabaseContext.ParsedPostAttributePictures.CountAsync(cancellationToken);
        statitics.ParsedPostAttributePicturesWithVector = await sqlDatabaseContext.ParsedPostAttributePictures
            .Where(postAttribute => postAttribute.IsVectorCreated == true)
            .CountAsync(cancellationToken);
        statitics.ParsedPostAttributePicturesWithoutVector = await sqlDatabaseContext.ParsedPostAttributePictures
            .Where(postAttribute => postAttribute.IsVectorCreated == false)
            .CountAsync(cancellationToken);

        statitics.ParsedPostAttributeEmbeds = await sqlDatabaseContext.ParsedPostAttributeEmbeds.CountAsync(cancellationToken);

        statitics.ParsedBandCamps = await sqlDatabaseContext.ParsedBandCamps.CountAsync(cancellationToken);
        statitics.ParsedCoubs = await sqlDatabaseContext.ParsedCoubs.CountAsync(cancellationToken);
        statitics.ParsedSoundClouds = await sqlDatabaseContext.ParsedSoundClouds.CountAsync(cancellationToken);
        statitics.ParsedVimeos = await sqlDatabaseContext.ParsedVimeos.CountAsync(cancellationToken);
        statitics.ParsedYouTubes = await sqlDatabaseContext.ParsedYouTubes.CountAsync(cancellationToken);

        return statitics;
    }
}