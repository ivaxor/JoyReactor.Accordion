using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Frozen;
using System.Net;

namespace JoyReactor.Accordion.Logic.Media.Images;

public class ImageDownloader(
    HttpClient httpClient,
    IImageReducer imageReducer,
    IOptions<ImageSettings> settings,
    ILogger<ImageDownloader> logger)
    : IImageDownloader
{
    protected readonly ResiliencePipeline ResiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<TimeoutRejectedException>()
                .Handle<HttpRequestException>(),
            MaxRetryAttempts = settings.Value.MaxRetryAttempts,
            Delay = settings.Value.RetryDelay,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                logger.LogWarning("Failed to send HTTP request to CDN. Attempt: {Attempt}/{MaxAttempts}. Message: {Message}. ", args.AttemptNumber, settings.Value.MaxRetryAttempts, args.Outcome.Exception?.Message);
                return default;
            }
        })
        .AddTimeout(TimeSpan.FromSeconds(10))
        .Build();

    protected static readonly ParsedPostAttributePictureType[] ImageTypes = [
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.BMP,
        ParsedPostAttributePictureType.TIFF,
    ];

    protected static readonly FrozenDictionary<ParsedPostAttributePictureType, string> ImageTypeToExtensions = ImageTypes
        .ToDictionary(type => type, type => Enum.GetName(type).ToLowerInvariant())
        .ToFrozenDictionary();

    protected static readonly FrozenSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/tiff",
        "image/bmp",
    }.ToFrozenSet();

    public async Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken)
    {
        if (!ImageTypes.Contains(picture.ImageType))
            throw new ArgumentOutOfRangeException(nameof(picture), "Unsupported picture type");

        var path = $"pics/post/picture-{picture.AttributeId}.{ImageTypeToExtensions[picture.ImageType]}";

        foreach (var cdnDomainName in settings.Value.CdnDomainNames)
        {
            var url = $"{cdnDomainName}/{path}";

            var image = await ResiliencePipeline.ExecuteAsync(async ct =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (mediaType == null || !AllowedMimeTypes.Contains(mediaType))
                    return null;

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await imageReducer.ReduceAsync(stream, cancellationToken);
            }, cancellationToken);

            if (image == null)
                continue;

            return image;
        }

        throw new FileNotFoundException("Failed to find picture on all CDN hosts");
    }
}

public interface IImageDownloader
{
    Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken);
}