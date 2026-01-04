using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Frozen;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class PicturesWithoutVectorCrawler(
    SqlDatabaseContext sqlDatabaseContext,
    IImageDownloader imageDownloader,
    IOnnxVectorConverter oonxVectorConverter,
    IVectorDatabaseContext vectorDatabaseContext,
    IOptions<CrawlerSettings> settings,
    ILogger<PicturesWithoutVectorCrawler> logger)
    : ScopedBackgroudService
{
    internal readonly PeriodicTimer PeriodicTimer = new PeriodicTimer(settings.Value.SubsequentRunDelay);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var imageTypes = new ParsedPostAttributePictureType[] {
            ParsedPostAttributePictureType.PNG,
            ParsedPostAttributePictureType.JPEG,
            ParsedPostAttributePictureType.BMP,
            ParsedPostAttributePictureType.TIFF,
        };
        var imageTypeToExtensions = imageTypes.ToDictionary(type => type, type => Enum.GetName(type)!).ToFrozenDictionary();

        var unprocessedPictures = (ParsedPostAttributePicture[])null;
        do
        {
            unprocessedPictures = await sqlDatabaseContext.ParsedPostAttributePictures
                .Where(picture => picture.IsVectorCreated == false && imageTypes.Contains(picture.ImageType))
                .OrderByDescending(picture => picture.Id)
                .Take(100)
                .ToArrayAsync(cancellationToken);

            if (unprocessedPictures.Length != 0)
                logger.LogInformation("Starting crawling {PicturesCount} pictures without vectors", unprocessedPictures.Length);
            else
            {

                logger.LogInformation("No pictures without vectors found. Will try again later");
                continue;
            }

            var pictureVectors = unprocessedPictures.ToDictionary(picture => picture, picture => (float[])null);
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

            await vectorDatabaseContext.UpsertAsync(pictureVectors, cancellationToken);
            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(unprocessedPictures);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
        } while (unprocessedPictures.Length != 0 || await PeriodicTimer.WaitForNextTickAsync(cancellationToken));
    }
}