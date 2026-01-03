using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.Workers.BackgroudServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);
Console.OutputEncoding = System.Text.Encoding.Unicode;

builder.Services.Configure<ApiClientSettings>(builder.Configuration.GetSection(nameof(ApiClientSettings)));
builder.Services.Configure<ImageSettings>(builder.Configuration.GetSection(nameof(ImageSettings)));
builder.Services.Configure<OnnxSettings>(builder.Configuration.GetSection(nameof(OnnxSettings)));
builder.Services.Configure<PostgreSqlSettings>(builder.Configuration.GetSection(nameof(PostgreSqlSettings)));
builder.Services.Configure<QdrantSettings>(builder.Configuration.GetSection(nameof(QdrantSettings)));

builder.Services.AddHttpClient();

builder.Services.AddSingleton(serviceProvider => new JsonSerializerOptions() { });

builder.Services.AddSingleton<IGraphQLClient, GraphQLHttpClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<ApiClientSettings>>();
    return new GraphQLHttpClient(settings.Value.GraphQlEndpointUrl, new SystemTextJsonSerializer());
});

builder.Services.AddDbContext<SqlDatabaseContext>((serviceProvider, options) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<PostgreSqlSettings>>();
    options.UseNpgsql(settings.Value.ConnectionString, npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName));
});

builder.Services.AddSingleton<IQdrantClient, QdrantClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<QdrantSettings>>();
    var client = new QdrantClient(settings.Value.Host);

    var isCollectionExists = client.CollectionExistsAsync(settings.Value.CollectionName).GetAwaiter().GetResult();
    if (!isCollectionExists)
    {
        client.CreateCollectionAsync(
            settings.Value.CollectionName,
            new VectorParams
            {
                Size = settings.Value.CollectionVectorSize,
                Distance = Distance.Cosine,
                Datatype = Datatype.Float32,
            },
            quantizationConfig: new QuantizationConfig
            {
                Scalar = new ScalarQuantization
                {
                    Type = QuantizationType.Int8,
                    AlwaysRam = true,
                }
            }).GetAwaiter().GetResult();
    }

    return client;
});

builder.Services.AddSingleton(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<OnnxSettings>>();

    var options = new SessionOptions();
    if (settings.Value.UseCpu)
        options.AppendExecutionProvider_CPU();
    if (settings.Value.UseCuda)
        options.AppendExecutionProvider_CUDA(int.Parse(settings.Value.DeviceId));
    if (settings.Value.UseRocm)
        options.AppendExecutionProvider_ROCm(int.Parse(settings.Value.DeviceId));
    if (settings.Value.UseOpenVino)
        options.AppendExecutionProvider_OpenVINO(settings.Value.DeviceId);

    return new InferenceSession(settings.Value.ModelPath, options);
});

builder.Services.AddSingleton<IApiClient, ApiClient>();
builder.Services.AddSingleton<ITagClient, TagClient>();
builder.Services.AddSingleton<IPostClient, PostClient>();

builder.Services.AddSingleton<IImageDownloader, ImageDownloader>();
builder.Services.AddHttpClient<IImageDownloader>(httpClient =>
{
    httpClient.DefaultRequestHeaders.Add("Referer", "https://joyreactor.cc");
});

builder.Services.AddSingleton<IOnnxVectorConverter, OnnxVectorConverter>();
builder.Services.AddSingleton<IVectorDatabaseContext, VectorDatabaseContext>();

builder.Services.AddHostedService<MainTagsCrawler>();
builder.Services.AddHostedService<EmptySubTagsCrawler>();
builder.Services.AddHostedService<TopWeekPostsCrawler>();
//builder.Services.AddHostedService<UnprocessedPictureVectorWorker>();

var host = builder.Build();

using var serviceScope = host.Services.CreateScope();
using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
await sqlDatabaseContext.Database.EnsureCreatedAsync();

host.Run();