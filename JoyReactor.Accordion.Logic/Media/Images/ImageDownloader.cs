using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Frozen;

namespace JoyReactor.Accordion.Logic.Media.Images;

public class ImageDownloader(
    HttpClient httpClient,
    IOptions<ImageSettings> settings)
    : IImageDownloader
{
    internal static readonly ParsedPostAttributePictureType[] ImageTypes = [
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.JPEG,
        ParsedPostAttributePictureType.PNG,
        ParsedPostAttributePictureType.TIFF,
    ];
    internal static readonly FrozenDictionary<ParsedPostAttributePictureType, string> ImageTypeToExtensions = ImageTypes
        .ToDictionary(type => type, type => Enum.GetName(type).ToLowerInvariant())
        .ToFrozenDictionary();

    internal readonly ResizeOptions ResizeOptions = new()
    {
        Size = new Size(settings.Value.ResizedSize, settings.Value.ResizedSize),
        Mode = ResizeMode.Pad,
        Sampler = KnownResamplers.Lanczos3,
    };

    public async Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken)
    {
        var path = $"/pics/post/picture-{picture.AttributeId}.{ImageTypeToExtensions[picture.ImageType]}";

        foreach (var cdnDomainName in settings.Value.CdnDomainNames)
        {
            var url = $"{cdnDomainName}/{path}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                continue;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var image = await Image.LoadAsync<Rgb24>(stream, cancellationToken);
            image.Mutate(x => x.Resize(ResizeOptions));

            return image;
        }

        throw new NotImplementedException();
    }
}

public interface IImageDownloader
{
    Task<Image<Rgb24>> DownloadAsync(ParsedPostAttributePicture picture, CancellationToken cancellationToken);
}