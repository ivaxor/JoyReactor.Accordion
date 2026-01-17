using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Database.Vector.Entities;
using JoyReactor.Accordion.Logic.Database.Vector.Extensions;
using JoyReactor.Accordion.Logic.Media;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using System.Collections.Frozen;
using System.Net.Mime;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("search/media")]
[ApiController]
[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
public class SearchMediaController(
    HttpClient httpClient,
    IMediaReducer mediaReducer,
    IOnnxVectorConverter onnxVectorConverter,
    IQdrantClient qdrantClient,
    IOptions<QdrantSettings> qdrantSettings)
    : ControllerBase
{
    protected const int FileSizeLimit = 5 * 1024 * 1024;
    protected static readonly FrozenSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        MediaTypeNames.Image.Png,
        MediaTypeNames.Image.Jpeg,
        MediaTypeNames.Image.Gif,
        MediaTypeNames.Image.Bmp,
        MediaTypeNames.Image.Tiff,
        "video/mp4",
        "video/webm",
    }.ToFrozenSet();
    protected static readonly FrozenSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "png",
        "jpeg",
        "jpg",
        "gif",
        "tiff",
        "bmp",
        "mp4",
        "webm",
    }.ToFrozenSet();

    [HttpPost("download")]
    [AllowAnonymous]
    [ProducesResponseType<PictureScoredPoint[]>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAsync([FromBody] SearchDownloadRequest request, CancellationToken cancellationToken = default)
    {
        using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, request.MediaUrl);
        var pictureUri = new Uri(request.MediaUrl);

        if (pictureUri.Host.EndsWith("reactor.cc", StringComparison.OrdinalIgnoreCase))
            downloadRequest.Headers.Add("Referer", "https://reactor.cc");
        else if (pictureUri.Host.EndsWith("joyreactor.cc", StringComparison.OrdinalIgnoreCase))
            downloadRequest.Headers.Add("Referer", "https://joyreactor.cc");
        else if (pictureUri.Host.EndsWith("joyreactor.com", StringComparison.OrdinalIgnoreCase))
            downloadRequest.Headers.Add("Referer", "https://joyreactor.com");

        var downloadResponse = await httpClient.SendAsync(downloadRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!downloadResponse.IsSuccessStatusCode)
        {
            ModelState.AddModelError(nameof(request.MediaUrl), "Failed to download file using provided url");
            return BadRequest(ModelState);
        }

        if (downloadResponse.Content.Headers.ContentLength >= FileSizeLimit)
        {
            ModelState.AddModelError(nameof(request.MediaUrl), $"Media size is too big. Max size is {FileSizeLimit} bytes");
            return BadRequest(ModelState);
        }

        var mediaType = downloadResponse.Content.Headers.ContentType?.MediaType;
        if (mediaType == null || !AllowedMimeTypes.Contains(mediaType))
        {
            ModelState.AddModelError(nameof(request.MediaUrl), "Media type is not allowed");
            return BadRequest(ModelState);
        }

        await using var stream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
        var results = await SearchAsync(mediaType, stream, cancellationToken);
        return Ok(results);
    }

    [HttpPost("upload")]
    [AllowAnonymous]
    [RequestSizeLimit(FileSizeLimit)]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [ProducesResponseType<PictureScoredPoint[]>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> SearchAsync([FromForm] SearchUploadRequest request, CancellationToken cancellationToken = default)
    {
        if (!AllowedMimeTypes.Contains(request.Media.ContentType))
        {
            ModelState.AddModelError(nameof(request.Media), "Media type is not allowed");
            return BadRequest(ModelState);
        }

        var mediaExtension = Path.GetExtension(request.Media.FileName).TrimStart('.');
        if (!AllowedExtensions.Contains(mediaExtension))
        {
            ModelState.AddModelError(nameof(request.Media), "Media extension type is not allowed");
            return BadRequest(ModelState);
        }

        await using var stream = request.Media.OpenReadStream();
        var results = await SearchAsync(request.Media.ContentType, stream, cancellationToken);
        return Ok(results);
    }

    protected async Task<PictureScoredPoint[]> SearchAsync(string mimeType, Stream stream, CancellationToken cancellationToken)
    {
        await using var boundedStream = new FileBufferingReadStream(stream, FileSizeLimit);
        await boundedStream.DrainAsync(cancellationToken);
        boundedStream.Position = 0;

        using var processedImage = await mediaReducer.ReduceAsync(mimeType, boundedStream, cancellationToken);
        var vector = await onnxVectorConverter.ConvertAsync(processedImage);
        var results = await qdrantClient.SearchAsync(qdrantSettings.Value.CollectionName, qdrantSettings.Value.SearchLimit, qdrantSettings.Value.SearchScoreThreshold, vector, cancellationToken);

        return results;
    }
}