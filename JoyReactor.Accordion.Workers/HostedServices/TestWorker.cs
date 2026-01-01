using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using Microsoft.Extensions.Hosting;

namespace JoyReactor.Accordion.Workers.HostedServices;

public class TestWorker(
    IPostClient postClient,
    IImageDownloader imageDownloader,
    IOnnxVectorConverter onnxVectorConverter,
    IVectorDatabaseContext vectorDatabaseContext)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var post = await postClient.GetAsync(6234782, cancellationToken);
        using var image = await imageDownloader.DownloadAsync(post.Attributes.First(), cancellationToken);
        var vector = await onnxVectorConverter.Convert(image.Value);
        await vectorDatabaseContext.InsertAsync(vector, cancellationToken);

        Console.WriteLine(vector.Length);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}