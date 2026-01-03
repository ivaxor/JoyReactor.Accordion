using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Frozen;

namespace JoyReactor.Accordion.Workers.BackgroudServices;

public class UnprocessedPictureVectorWorker : BackgroundService, IDisposable
{
    internal static readonly ParsedPostAttributePictureType[] ImageTypes = [
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.TIFF,
    ];
    internal static readonly FrozenDictionary<ParsedPostAttributePictureType, string> ImageTypeToExtensions = ImageTypes
        .ToDictionary(type => type, type => Enum.GetName(type)!)
        .ToFrozenDictionary();

    internal readonly IImageDownloader imageDownloader;
    internal readonly IOnnxVectorConverter oonxVectorConverter;
    internal readonly IVectorDatabaseContext vectorDatabaseContext;
    internal readonly IServiceScope serviceScope;
    internal readonly SqlDatabaseContext sqlDatabaseContext;

    public UnprocessedPictureVectorWorker(
        IImageDownloader imageDownloader,
        IOnnxVectorConverter oonxVectorConverter,
        IVectorDatabaseContext vectorDatabaseContext,
        IServiceProvider serviceProvider)
    {
        this.imageDownloader = imageDownloader;
        this.oonxVectorConverter = oonxVectorConverter;
        this.vectorDatabaseContext = vectorDatabaseContext;

        serviceScope = serviceProvider.CreateScope();
        sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
    }

    public void Dispose()
    {
        serviceScope.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        var unprocessedPictures = (ParsedPostAttributePicture[])null;

        do
        {
            unprocessedPictures = await sqlDatabaseContext.ParsedPostAttributePictures
                .Where(picture => picture.IsVectorCreated == false && ImageTypes.Contains(picture.ImageType))
                .Take(100)
                .ToArrayAsync(cancellationToken);

            foreach (var pictures in unprocessedPictures.Chunk(10))
                await Task.WhenAll(pictures.Select(picture => CrawlAsync(picture, cancellationToken)).ToArray());

            foreach (var picture in unprocessedPictures)
            {
                picture.IsVectorCreated = true;
                picture.UpdatedAt = DateTime.Now;
            }

            sqlDatabaseContext.ParsedPostAttributePictures.UpdateRange(unprocessedPictures);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
        } while (unprocessedPictures.Length != 0 || await periodicTimer.WaitForNextTickAsync(cancellationToken));
    }

    internal async Task CrawlAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken)
    {
        using var image = await imageDownloader.DownloadAsync(picture, cancellationToken);
        var vector = await oonxVectorConverter.ConvertAsync(image);
        await vectorDatabaseContext.InsertAsync(picture, vector, cancellationToken);
    }
}