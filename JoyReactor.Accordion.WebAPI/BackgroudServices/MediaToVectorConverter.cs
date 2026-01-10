using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class MediaToVectorConverter(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<MediaSettings> mediaSettings,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<MediaToVectorConverter> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected static readonly ParsedPostAttributePictureType[] ImageTypes = [
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.GIF,
        ParsedPostAttributePictureType.BMP,
        ParsedPostAttributePictureType.TIFF,
        //ParsedPostAttributePictureType.MP4,
        //ParsedPostAttributePictureType.WEBM,
    ];

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        var unprocessedPictures = (ParsedPostAttributePicture[])null;
        do
        {
            await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
            await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
            var mediaDownloader = serviceScope.ServiceProvider.GetRequiredService<IMediaDownloader>();
            var onnxVectorConverter = serviceScope.ServiceProvider.GetRequiredService<IOnnxVectorConverter>();
            var vectorDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IVectorDatabaseContext>();

            unprocessedPictures = await sqlDatabaseContext.ParsedPostAttributePictures
                .Where(picture => picture.IsVectorCreated == false && ImageTypes.Contains(picture.ImageType))
                .OrderByDescending(picture => picture.Id)
                .Take(mediaSettings.Value.ConcurrentDownloads * 10)
                .ToArrayAsync(cancellationToken);

            if (unprocessedPictures.Length == 0)
            {
                logger.LogInformation("No post attribute pictures without vector found. Will try again later.");
                return;
            }
            logger.LogInformation("Starting crawling {PicturesCount} post attribute pictures without vectors.", unprocessedPictures.Length);

            var pictureVectors = new ConcurrentDictionary<ParsedPostAttributePicture, float[]>();
            foreach (var pictures in unprocessedPictures.Chunk(mediaSettings.Value.ConcurrentDownloads))
            {
                var pictureImages = new ConcurrentDictionary<ParsedPostAttributePicture, Image<Rgb24>>();
                await Task.WhenAll(pictures.Select(picture => DownloadAsync(mediaDownloader, pictureImages, picture, cancellationToken)));

                foreach (var (picture, image) in pictureImages)
                {
                    await CreateVectorAsync(onnxVectorConverter, pictureVectors, picture, image);
                }

                logger.LogInformation("Chuck of {PicturesCount} picture post attributes were converted to vectors.", pictures.Length);
            }

            await vectorDatabaseContext.UpsertAsync(pictureVectors, cancellationToken);
            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(pictureVectors.Keys);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{PicturesCount} picture post attributes were converted to vectors.", pictureVectors.Count);
        } while (unprocessedPictures.Length != 0);
    }

    protected async Task DownloadAsync(
        IMediaDownloader mediaDownloader,
        IDictionary<ParsedPostAttributePicture, Image<Rgb24>> pictureImages,
        ParsedPostAttributePicture picture,
        CancellationToken cancellationToken)
    {
        try
        {
            var image = await mediaDownloader.DownloadAsync(picture, cancellationToken);
            pictureImages.TryAdd(picture, image);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download {PictureAttributeId} post attribute picture", picture.AttributeId);
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
            logger.LogError(ex, "Failed to create vector for {PictureAttributeId} post attribute picture", picture.AttributeId);
        }
        finally
        {
            image?.Dispose();
        }
    }
}