using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Diagnostics;

namespace JoyReactor.Accordion.Workers.BackgroudServices;

public class UnprocessedPictureVectorCrawler(
    SqlDatabaseContext sqlDatabaseContext,
    IImageDownloader imageDownloader,
    IOnnxVectorConverter oonxVectorConverter,
    IVectorDatabaseContext vectorDatabaseContext,
    ILogger<UnprocessedPictureVectorCrawler> logger)
    : ScopedBackgroudService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var imageTypes = new ParsedPostAttributePictureType[] {
            ParsedPostAttributePictureType.PNG,
            ParsedPostAttributePictureType.JPEG,
            ParsedPostAttributePictureType.BMP,
            ParsedPostAttributePictureType.TIFF,
        };
        var imageTypeToExtensions = imageTypes.ToDictionary(type => type, type => Enum.GetName(type)!).ToFrozenDictionary();

        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        var unprocessedPictures = (ParsedPostAttributePicture[])null;

        do
        {
            unprocessedPictures = await sqlDatabaseContext.ParsedPostAttributePictures
                .Where(picture => picture.IsVectorCreated == false && imageTypes.Contains(picture.ImageType))
                .OrderByDescending(picture => picture.Id)
                .Take(100)
                .ToArrayAsync(cancellationToken);
            logger.LogInformation("Starting crawling {PicturesCount} pictures without vectors", unprocessedPictures.Length);

            if (unprocessedPictures.Length == 0)
                continue;

            var pictureVectors = unprocessedPictures.ToDictionary(picture => picture, picture => (float[])null);

            var stopwatch = Stopwatch.StartNew();
            foreach (var picture in pictureVectors.Keys)
            {
                try
                {
                    using var image = await imageDownloader.DownloadAsync(picture, cancellationToken);
                    pictureVectors[picture] = await oonxVectorConverter.ConvertAsync(image);

                    picture.IsVectorCreated = true;
                    picture.UpdatedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to crawl {PictureAttributeId} picture without vector", picture.AttributeId);
                }
            }
            logger.LogInformation("Created vectors for {PicturesCount} pictures. Average processing time is {ElapsedMilliseconds} ms per image", unprocessedPictures.Length, stopwatch.ElapsedMilliseconds / unprocessedPictures.Length);

            await vectorDatabaseContext.UpsertAsync(pictureVectors, cancellationToken);
            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(unprocessedPictures);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
        } while (unprocessedPictures.Length != 0 || await periodicTimer.WaitForNextTickAsync(cancellationToken));
    }
}