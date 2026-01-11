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

namespace JoyReactor.Accordion.Logic.Media;

public class MediaDownloader(
    HttpClient httpClient,
    IMediaReducer mediaReducer,
    IOptions<MediaSettings> settings,
    ILogger<MediaDownloader> logger)
    : IMediaDownloader
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
                logger.LogWarning("Failed to send HTTP request to CDN. Attempt: {Attempt}/{MaxAttempts}. Message: {Message}.", args.AttemptNumber, settings.Value.MaxRetryAttempts, args.Outcome.Exception?.Message);
                return default;
            },
        })
        .AddTimeout(TimeSpan.FromSeconds(100))
        .Build();

    protected static readonly FrozenSet<ParsedPostAttributePictureType> PictureTypes = new HashSet<ParsedPostAttributePictureType>() {
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.GIF,
        ParsedPostAttributePictureType.BMP,
        ParsedPostAttributePictureType.TIFF,
        ParsedPostAttributePictureType.MP4,
        ParsedPostAttributePictureType.WEBM,
    }.ToFrozenSet();

    protected static readonly FrozenDictionary<ParsedPostAttributePictureType, string> PictureTypeToExtensions = PictureTypes
        .ToDictionary(type => type, type => Enum.GetName(type).ToLowerInvariant())
        .ToFrozenDictionary();

    protected static readonly FrozenSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/gif",
        "image/bmp",
        "image/tiff",
        "image/mp4",
        "image/webm",
    }.ToFrozenSet();

    public async Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken)
    {
        if (!PictureTypes.Contains(picture.ImageType))
            throw new ArgumentOutOfRangeException(nameof(picture), "Unsupported media type");

        var path = $"pics/post/picture-{picture.AttributeId}.{PictureTypeToExtensions[picture.ImageType]}";
        foreach (var cdnDomainName in settings.Value.CdnDomainNames)
        {
            var url = $"{cdnDomainName}/{path}";

            var image = await ResiliencePipeline.ExecuteAsync(async ct => await DownloadAsync(url, picture, ct), cancellationToken);
            if (image == null)
                continue;

            return image;
        }

        throw new FileNotFoundException("Failed to download media from all CDNs");
    }

    protected async Task<Image<Rgb24>> DownloadAsync(string url, ParsedPostAttributePicture picture, CancellationToken cancellationToken)
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
        return await mediaReducer.ReduceAsync(picture, stream, cancellationToken);
    }
}

public interface IMediaDownloader
{
    Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken);
}