using FFMpegCore;
using FFMpegCore.Pipes;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Net.Mime;
using System.Reflection;

namespace JoyReactor.Accordion.Logic.Media;

public class MediaReducer(IOptions<MediaSettings> settings)
    : IMediaReducer
{
    protected readonly ResizeOptions ResizeOptions = new()
    {
        Size = new Size(settings.Value.ResizedSize, settings.Value.ResizedSize),
        Mode = ResizeMode.Pad,
        Sampler = KnownResamplers.Lanczos3,
    };

    public Task<Image<Rgb24>> ReduceAsync(ParsedPostAttributePicture picture, Stream stream, CancellationToken cancellationToken)
    {
        switch (picture.ImageType)
        {
            case ParsedPostAttributePictureType.PNG:
            case ParsedPostAttributePictureType.JPEG:
            case ParsedPostAttributePictureType.GIF:
            case ParsedPostAttributePictureType.BMP:
            case ParsedPostAttributePictureType.TIFF:
                return ReducePictureAsync(stream, cancellationToken);

            case ParsedPostAttributePictureType.MP4:
            case ParsedPostAttributePictureType.WEBM:
                return ReduceVideoAsync(stream, cancellationToken);

            default:
                throw new NotImplementedException();
        }
    }

    public Task<Image<Rgb24>> ReduceAsync(string mimeType, Stream stream, CancellationToken cancellationToken)
    {
        switch (mimeType)
        {
            case MediaTypeNames.Image.Png:
            case MediaTypeNames.Image.Jpeg:
            case MediaTypeNames.Image.Gif:
            case MediaTypeNames.Image.Bmp:
            case MediaTypeNames.Image.Tiff:
                return ReducePictureAsync(stream, cancellationToken);

            case "video/mp4":
            case "video/webm":
                return ReduceVideoAsync(stream, cancellationToken);

            default:
                throw new NotImplementedException();
        }
    }

    protected async Task<Image<Rgb24>> ReducePictureAsync(Stream stream, CancellationToken cancellationToken)
    {
        var image = await Image.LoadAsync<Rgb24>(stream, cancellationToken);
        if (image.Frames.Count <= 1)
        {
            image.Mutate(x => x.Resize(ResizeOptions));
            return image;
        }

        try
        {
            var frameIndex = Convert.ToInt32(Math.Round(image.Frames.Count / 2.0));
            if (frameIndex >= image.Frames.Count)
                frameIndex = image.Frames.Count - 1;

            var singleFrameImage = image.Frames.CloneFrame(frameIndex);
            singleFrameImage.Mutate(x => x.Resize(ResizeOptions));

            return singleFrameImage;
        }
        finally
        {
            image.Dispose();
        }
    }

    protected async Task<Image<Rgb24>> ReduceVideoAsync(Stream stream, CancellationToken cancellationToken)
    {
        var folderPath = Path.Combine(Path.GetTempPath(), Assembly.GetEntryAssembly().GetName().Name);
        Directory.CreateDirectory(folderPath);
        var filePath = Path.Combine(folderPath, Guid.NewGuid().ToString());

        try
        {
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, cancellationToken);
            await fileStream.FlushAsync(cancellationToken);
            fileStream.Close();

            var mediaAnalysis = await FFProbe.AnalyseAsync(filePath, cancellationToken: cancellationToken);

            await using var imageStream = new MemoryStream();
            await FFMpegArguments
                .FromFileInput(filePath, addArguments: options => options
                    .Seek(mediaAnalysis.Duration / 2))
                .OutputToPipe(new StreamPipeSink(imageStream), options => options
                    .WithFrameOutputCount(1)
                    .WithVideoCodec("mjpeg")
                    .ForceFormat("image2"))
                .ProcessAsynchronously();
            imageStream.Position = 0;

            var image = await Image.LoadAsync<Rgb24>(imageStream, cancellationToken);
            image.Mutate(x => x.Resize(ResizeOptions));
            return image;
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}

public interface IMediaReducer
{
    Task<Image<Rgb24>> ReduceAsync(ParsedPostAttributePicture picture, Stream stream, CancellationToken cancellationToken);
    Task<Image<Rgb24>> ReduceAsync(string mimeType, Stream stream, CancellationToken cancellationToken);
}