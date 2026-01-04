using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Frozen;

namespace JoyReactor.Accordion.Logic.Media.Images;

public class ImageDownloader(
    HttpClient httpClient,
    IImageReducer imageReducer,
    IOptions<ImageSettings> settings)
    : IImageDownloader
{
    internal static readonly ParsedPostAttributePictureType[] ImageTypes = [
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.BMP,
        ParsedPostAttributePictureType.TIFF,
    ];
    internal static readonly FrozenDictionary<ParsedPostAttributePictureType, string> ImageTypeToExtensions = ImageTypes
        .ToDictionary(type => type, type => Enum.GetName(type).ToLowerInvariant())
        .ToFrozenDictionary();

    public async Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken)
    {
        if (!ImageTypes.Contains(picture.ImageType))
            throw new ArgumentOutOfRangeException(nameof(picture), "Unsupported picture type");

        var path = $"pics/post/picture-{picture.AttributeId}.{ImageTypeToExtensions[picture.ImageType]}";
        foreach (var cdnDomainName in settings.Value.CdnDomainNames)
        {
            var url = $"{cdnDomainName}/{path}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                continue;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await imageReducer.ReduceAsync(stream, cancellationToken);
        }

        throw new NotImplementedException();
    }
}

public interface IImageDownloader
{
    Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken);
}