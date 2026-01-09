using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;

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
            var onnxVectorConverter = serviceScope.ServiceProvider.GetRequiredService<IOnnxVectorConverter>();
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

            var pictureVectors = new ConcurrentDictionary<ParsedPostAttributePicture, float[]>();
            foreach (var pictures in unprocessedPictures.Chunk(10))
            {
                var pictureImages = new ConcurrentDictionary<ParsedPostAttributePicture, Image<Rgb24>>();
                await Task.WhenAll(pictures.Select(picture => DownloadImageAsync(imageDownloader, pictureImages, picture, cancellationToken)));

                foreach (var (picture, image) in pictureImages)
                {
                    await CreateVectorAsync(onnxVectorConverter, pictureVectors, picture, image);
                }
            }

            await vectorDatabaseContext.UpsertAsync(pictureVectors, cancellationToken);
            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(pictureVectors.Keys);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
        } while (unprocessedPictures.Length != 0);
    }

    protected async Task DownloadImageAsync(
        IImageDownloader imageDownloader,
        IDictionary<ParsedPostAttributePicture, Image<Rgb24>> pictureImages,
        ParsedPostAttributePicture picture,
        CancellationToken cancellationToken)
    {
        try
        {
            var image = await imageDownloader.DownloadAsync(picture, cancellationToken);
            pictureImages.TryAdd(picture, image);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download {PictureAttributeId} picture", picture.AttributeId);
        }
    }

    protected async Task CreateVectorAsync(
        IOnnxVectorConverter onnxVectorConverter,
        IDictionary<ParsedPostAttributePicture, float[]> pictureVectors,
        ParsedPostAttributePicture picture,
        Image<Rgb24> image)
    {
        try
        {
            var vector = await onnxVectorConverter.ConvertAsync(image);
            if (pictureVectors.TryAdd(picture, vector))
            {
                picture.IsVectorCreated = true;
                picture.UpdatedAt = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create vector for {PictureAttributeId} picture", picture.AttributeId);
        }
        finally
        {
            image?.Dispose();
        }
    }
}