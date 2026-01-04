using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Database.Vector.Entities;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Frozen;

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SearchController(
    HttpClient httpClient,
    IImageReducer imageReducer,
    IOnnxVectorConverter onnxVectorConverter,
    IVectorDatabaseContext vectorDatabaseContext)
    : ControllerBase
{
    internal const int FileSizeLimit = 5 * 1024 * 1024;
    internal static readonly FrozenSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/octet-stream",
        "image/png",
        "image/jpeg",
        "image/tiff",
        "image/bmp",
    }.ToFrozenSet();
    internal static readonly FrozenSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "png",
        "jpeg",
        "jpg",
        "tiff",
        "bmp",
    }.ToFrozenSet();

    [HttpPost("search/download")]
    public async Task<IActionResult> SearchPictureAsync([FromBody] SearchDownloadRequest request, CancellationToken cancellationToken = default)
    {
        using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, request.PictureUrl);
        /*
        if (request.PictureUrl.Host.Contains("joyreactor.cc", StringComparison.OrdinalIgnoreCase))
            downloadRequest.Headers.Add("Referer", "https://joyreactor.cc");
        */

        var downloadResponse = await httpClient.SendAsync(downloadRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!downloadResponse.IsSuccessStatusCode)
            return BadRequest("Failed to download file");

        if (downloadResponse.Content.Headers.ContentLength >= FileSizeLimit)
            return BadRequest("File is too big");

        var mediaType = downloadResponse.Content.Headers.ContentType?.MediaType;
        if (mediaType == null || !AllowedMimeTypes.Contains(mediaType))
            return BadRequest("Unsupported picture file type");

        await using var stream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
        var results = await SearchAsync(stream, cancellationToken);
        return Ok(results);
    }

    [RequestSizeLimit(FileSizeLimit)]
    [HttpPost("search/upload")]
    public async Task<IActionResult> SearchPictureAsync([FromForm] SearchUploadRequest request, CancellationToken cancellationToken = default)
    {
        if (!AllowedMimeTypes.Contains(request.Picture.ContentType))
            return BadRequest("Unsupported picture file type");

        var pictureExtension = Path.GetExtension(request.Picture.FileName).TrimStart('.');
        if (!AllowedExtensions.Contains(pictureExtension))
            return BadRequest("Unsupported picture file extension");

        await using var stream = request.Picture.OpenReadStream();
        var results = await SearchAsync(stream, cancellationToken);
        return Ok(results);
    }

    internal async Task<VectorSearchResult[]> SearchAsync(Stream stream, CancellationToken cancellationToken)
    {
        await using var boundedStream = new FileBufferingReadStream(stream, FileSizeLimit);
        await boundedStream.DrainAsync(cancellationToken);
        boundedStream.Position = 0;

        using var processedImage = await imageReducer.ReduceAsync(boundedStream, cancellationToken);
        var vector = await onnxVectorConverter.ConvertAsync(processedImage);
        var results = await vectorDatabaseContext.SearchAsync(vector, cancellationToken);

        return results;
    }
}