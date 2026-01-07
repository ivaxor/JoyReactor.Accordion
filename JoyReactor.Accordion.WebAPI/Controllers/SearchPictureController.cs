using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Database.Vector.Entities;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Frozen; 

namespace JoyReactor.Accordion.WebAPI.Controllers;

[Route("api/search/pictures")]
[ApiController]
public class SearchPictureController(
    HttpClient httpClient,
    IImageReducer imageReducer,
    IOnnxVectorConverter onnxVectorConverter,
    IVectorDatabaseContext vectorDatabaseContext)
    : ControllerBase
{
    protected const int FileSizeLimit = 5 * 1024 * 1024;
    protected static readonly FrozenSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/octet-stream",
        "image/png",
        "image/jpeg",
        "image/tiff",
        "image/bmp",
    }.ToFrozenSet();
    protected static readonly FrozenSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "png",
        "jpeg",
        "jpg",
        "tiff",
        "bmp",
    }.ToFrozenSet();

    [HttpPost("download")]
    public async Task<IActionResult> SearchAsync([FromBody] SearchDownloadRequest request, CancellationToken cancellationToken = default)
    {
        using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, request.PictureUrl);
        if (request.PictureUrl.Host.Contains("joyreactor.cc", StringComparison.OrdinalIgnoreCase))
            downloadRequest.Headers.Add("Referer", "https://joyreactor.cc");

        var downloadResponse = await httpClient.SendAsync(downloadRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!downloadResponse.IsSuccessStatusCode)
        {
            ModelState.AddModelError(nameof(request.PictureUrl), "Failed to download file using provided url");
            return BadRequest(ModelState);
        }

        if (downloadResponse.Content.Headers.ContentLength >= FileSizeLimit)
        {
            ModelState.AddModelError(nameof(request.PictureUrl), $"Picture size is too big. Max size is {FileSizeLimit} bytes");
            return BadRequest(ModelState);
        }

        var mediaType = downloadResponse.Content.Headers.ContentType?.MediaType;
        if (mediaType == null || !AllowedMimeTypes.Contains(mediaType))
        {
            ModelState.AddModelError(nameof(request.PictureUrl), "Picture media type is not allowed");
            return BadRequest(ModelState);
        }

        await using var stream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
        var results = await SearchAsync(stream, cancellationToken);
        return Ok(results);
    }

    [RequestSizeLimit(FileSizeLimit)]
    [HttpPost("upload")]
    public async Task<IActionResult> SearchAsync([FromForm] SearchUploadRequest request, CancellationToken cancellationToken = default)
    {
        if (!AllowedMimeTypes.Contains(request.Picture.ContentType))
        {
            ModelState.AddModelError(nameof(request.Picture), "Picture media type is not allowed");
            return BadRequest(ModelState);
        }

        var pictureExtension = Path.GetExtension(request.Picture.FileName).TrimStart('.');
        if (!AllowedExtensions.Contains(pictureExtension))
        {
            ModelState.AddModelError(nameof(request.Picture), "Picture extension type is not allowed");
            return BadRequest(ModelState);
        }

        await using var stream = request.Picture.OpenReadStream();
        var results = await SearchAsync(stream, cancellationToken);
        return Ok(results);
    }

    protected async Task<PictureScoredPoint[]> SearchAsync(Stream stream, CancellationToken cancellationToken)
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