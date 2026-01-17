using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Database.Vector.Extensions;
using JoyReactor.Accordion.Logic.Media;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class MediaToVectorConverter(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<MediaSettings> mediaSettings,
    IOptions<QdrantSettings> qdrantSettings,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<MediaToVectorConverter> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected static readonly ParsedPostAttributePictureType[] SupportedImageTypes = [
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.GIF,
        ParsedPostAttributePictureType.BMP,
        ParsedPostAttributePictureType.TIFF,
        ParsedPostAttributePictureType.MP4,
        ParsedPostAttributePictureType.WEBM,
    ];

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        var unprocessedPictures = (ParsedPostAttributePicture[])null;
        var failedPictureAttributeIds = new HashSet<Guid>();

        do
        {
            await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
            await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
            var mediaDownloader = serviceScope.ServiceProvider.GetRequiredService<IMediaDownloader>();
            var onnxVectorConverter = serviceScope.ServiceProvider.GetRequiredService<IOnnxVectorConverter>();
            var qdrantClient = serviceScope.ServiceProvider.GetRequiredService<IQdrantClient>();

            unprocessedPictures = await sqlDatabaseContext.ParsedPostAttributePictures
                .Where(picture => picture.NoContent == false && picture.UnsupportedContent == false && picture.IsVectorCreated == false)
                .Where(picture => SupportedImageTypes.Contains(picture.ImageType))
                .Where(picture => !failedPictureAttributeIds.Contains(picture.Id))
                .Include(picture => picture.Post)
                .OrderBy(picture => picture.AttributeId)
                .Take(mediaSettings.Value.ConcurrentDownloads * 10)
                .ToArrayAsync(cancellationToken);

            if (unprocessedPictures.Length == 0)
            {
                logger.LogInformation("No post attribute pictures without vector found. Will try again later.");
                return;
            }
            logger.LogInformation("Starting crawling {PicturesCount} post attribute picture(s) without vectors.", unprocessedPictures.Length);

            var pictureVectors = new ConcurrentDictionary<ParsedPostAttributePicture, float[]>();
            foreach (var pictures in unprocessedPictures.Chunk(mediaSettings.Value.ConcurrentDownloads))
            {
                var pictureImages = new ConcurrentDictionary<ParsedPostAttributePicture, Image<Rgb24>>();
                await Task.WhenAll(pictures.Select(picture => DownloadAsync(mediaDownloader, failedPictureAttributeIds, pictureImages, picture, cancellationToken)));

                foreach (var (picture, image) in pictureImages)
                {
                    await CreateVectorAsync(onnxVectorConverter, failedPictureAttributeIds, pictureVectors, picture, image);
                }

                logger.LogInformation("Chunk of {PicturesCount} picture post attribute(s) were converted to vector(s).", pictures.Length);
            }

            var failedPictureVectors = unprocessedPictures.Where(picture => picture.NoContent || picture.UnsupportedContent).ToArray();

            await using var transaction = await sqlDatabaseContext.Database.BeginTransactionAsync(cancellationToken);
            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(pictureVectors.Keys);
            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(failedPictureVectors);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

            await qdrantClient.UpsertAsync(qdrantSettings.Value.CollectionName, pictureVectors, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("{PicturesCount} picture post attribute(s) were converted and saved as vector(s).", pictureVectors.Count);
        } while (unprocessedPictures.Length != 0);
    }

    protected async Task DownloadAsync(
        IMediaDownloader mediaDownloader,
        ISet<Guid> failedPictureAttributeIds,
        IDictionary<ParsedPostAttributePicture, Image<Rgb24>> pictureImages,
        ParsedPostAttributePicture picture,
        CancellationToken cancellationToken)
    {
        if (failedPictureAttributeIds.Contains(picture.Id))
            return;

        try
        {
            var image = await mediaDownloader.DownloadAsync(picture, cancellationToken);
            pictureImages.TryAdd(picture, image);
        }
        catch (FileNotFoundException ex)
        {
            // TODO: Try to download mp4/webm if download failed and type is GIF
            picture.NoContent = true;
            picture.UpdatedAt = DateTime.UtcNow;
            logger.LogWarning(ex, "Failed to download {PictureAttributeId} post attribute picture due to no content.", picture.AttributeId);
        }
        catch (InvalidImageContentException ex)
        {
            picture.UnsupportedContent = true;
            picture.UpdatedAt = DateTime.UtcNow;
            logger.LogWarning(ex, "Failed to create image for {PictureAttributeId} post attribute picture due to invalid image content.", picture.AttributeId);
        }
        catch (UnknownImageFormatException ex)
        {
            picture.UnsupportedContent = true;
            picture.UpdatedAt = DateTime.UtcNow;
            logger.LogWarning(ex, "Failed to create image for {PictureAttributeId} post attribute picture due to unknown image format.", picture.AttributeId);
        }
        catch (NotSupportedException ex)
        {
            picture.UnsupportedContent = true;
            picture.UpdatedAt = DateTime.UtcNow;
            logger.LogWarning(ex, "Failed to create image for {PictureAttributeId} post attribute picture due to unsupported content.", picture.AttributeId);
        }
        catch (Exception ex)
        {
            failedPictureAttributeIds.Add(picture.Id);
            logger.LogError(ex, "Failed to download {PictureAttributeId} post attribute picture. Adding it to temporary ignore list.", picture.AttributeId);
        }
    }

    protected async Task CreateVectorAsync(
        IOnnxVectorConverter onnxVectorConverter,
        ISet<Guid> failedPictureAttributeIds,
        IDictionary<ParsedPostAttributePicture, float[]> pictureVectors,
        ParsedPostAttributePicture picture,
        Image<Rgb24> image)
    {
        if (failedPictureAttributeIds.Contains(picture.Id))
            return;

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
            failedPictureAttributeIds.Add(picture.Id);
            logger.LogError(ex, "Failed to create vector for {PictureAttributeId} post attribute picture. Adding it to temporary ignore list.", picture.AttributeId);
        }
        finally
        {
            image?.Dispose();
        }
    }
}