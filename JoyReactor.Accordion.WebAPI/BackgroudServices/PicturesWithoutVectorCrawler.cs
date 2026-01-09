using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class PicturesWithoutVectorCrawler(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<PicturesWithoutVectorCrawler> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected static readonly ParsedPostAttributePictureType[] ImageTypes = [
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.BMP,
        ParsedPostAttributePictureType.TIFF,
    ];

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        var unprocessedPictures = (ParsedPostAttributePicture[])null;
        do
        {
            await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
            await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
            var imageDownloader = serviceScope.ServiceProvider.GetRequiredService<IImageDownloader>();
            var oonxVectorConverter = serviceScope.ServiceProvider.GetRequiredService<IOnnxVectorConverter>();
            var vectorDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IVectorDatabaseContext>();

            unprocessedPictures = await sqlDatabaseContext.ParsedPostAttributePictures
                .Where(picture => picture.IsVectorCreated == false && ImageTypes.Contains(picture.ImageType))
                .OrderByDescending(picture => picture.Id)
                .Take(100)
                .ToArrayAsync(cancellationToken);

            if (unprocessedPictures.Length != 0)
                logger.LogInformation("Starting crawling {PicturesCount} pictures without vectors.", unprocessedPictures.Length);
            else
            {
                logger.LogInformation("No pictures without vector found. Will try again later.");
                return;
            }

            var totalStopwatch = Stopwatch.StartNew();
            var totalDownloadStopwatch = new Stopwatch();
            var totalVectorStopwatch = new Stopwatch();
            var pictureVectors = new ConcurrentDictionary<ParsedPostAttributePicture, float[]>();
            foreach (var picture in unprocessedPictures)
            {
                try
                {
                    totalDownloadStopwatch.Start();
                    using var image = await imageDownloader.DownloadAsync(picture, cancellationToken);
                    totalDownloadStopwatch.Stop();

                    totalVectorStopwatch.Start();
                    var vector = await oonxVectorConverter.ConvertAsync(image);
                    totalVectorStopwatch.Stop();

                    if (pictureVectors.TryAdd(picture, vector))
                    {
                        picture.IsVectorCreated = true;
                        picture.UpdatedAt = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to crawl {PictureAttributeId} picture without vector.", picture.AttributeId);
                }
            }
            totalStopwatch.Stop();

            await vectorDatabaseContext.UpsertAsync(pictureVectors, cancellationToken);
            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(pictureVectors.Keys);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

            var avgDownloadTime = totalDownloadStopwatch.Elapsed / pictureVectors.Count();
            var avgVectorTime = totalVectorStopwatch.Elapsed / pictureVectors.Count();
            logger.LogInformation("Crawled {PicturesCount} pictures. Total time: {TotalTime}. Avg download time: {AvgDownloadTime}. Avg vector time: {AvgVectorTime}.",
                pictureVectors.Count,
                totalStopwatch.Elapsed,
                avgDownloadTime,
                avgVectorTime);
        } while (unprocessedPictures.Length != 0);
    }
}