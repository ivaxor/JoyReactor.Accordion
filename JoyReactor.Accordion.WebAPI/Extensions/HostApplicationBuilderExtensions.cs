using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.HealthChecks;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Serilog;
using System.Threading.RateLimiting;

namespace JoyReactor.Accordion.WebAPI.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static void AddLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Console();

            var elasticsearchUri = context.Configuration["Serilog:ElasticsearchUri"];
            if (!string.IsNullOrWhiteSpace(elasticsearchUri))
            {
                configuration.WriteTo.Elasticsearch([new Uri(elasticsearchUri)], options =>
                {
                    options.DataStream = new DataStreamName("logs", context.HostingEnvironment.ApplicationName.ToLower(), context.HostingEnvironment.EnvironmentName.ToLower());
                    options.BootstrapMethod = BootstrapMethod.Failure;
                });
            }

            if (context.HostingEnvironment.IsDevelopment())
                configuration.WriteTo.Debug();
        });
    }

    public static void AddOptionsFromConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ApiClientSettings>(builder.Configuration.GetSection(nameof(ApiClientSettings)));
        builder.Services.Configure<MediaSettings>(builder.Configuration.GetSection(nameof(MediaSettings)));
        builder.Services.Configure<OnnxSettings>(builder.Configuration.GetSection(nameof(OnnxSettings)));
        builder.Services.Configure<PostgreSqlSettings>(builder.Configuration.GetSection(nameof(PostgreSqlSettings)));
        builder.Services.Configure<QdrantSettings>(builder.Configuration.GetSection(nameof(QdrantSettings)));
        builder.Services.Configure<BackgroundServiceSettings>(builder.Configuration.GetSection(nameof(BackgroundServiceSettings)));
        builder.Services.Configure<RateLimiterSettings>(builder.Configuration.GetSection(nameof(RateLimiterSettings)));
    }

    public static void AddGraphQlClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IGraphQLClient, GraphQLHttpClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<ApiClientSettings>>();
            return new GraphQLHttpClient(settings.Value.GraphQlEndpointUrl, new SystemTextJsonSerializer());
        });
    }

    public static void AddDatabases(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<SqlDatabaseContext>((serviceProvider, options) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<PostgreSqlSettings>>();
            options.UseNpgsql(settings.Value.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
            });

            if (builder.Environment.IsDevelopment())
                options.EnableSensitiveDataLogging();
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
    }

    public static void AddInferenceSession(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<OnnxSettings>>();

            var options = new Microsoft.ML.OnnxRuntime.SessionOptions();
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
    }

    public static void AddRateLimiter(this IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
            return;

        builder.Services.AddRateLimiter(options =>
        {
            var settings = builder.Configuration
                .GetSection(nameof(RateLimiterSettings))
                .Get<RateLimiterSettings>();

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var realIp = httpContext.Request.Headers["X-Real-Ip"].FirstOrDefault();
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();

                var partitionKey = realIp ?? forwardedFor ?? remoteIp ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = settings.PermitLimit,
                        QueueLimit = 0,
                        Window = settings.Window,
                    });
            });
        });
    }

    public static void AddHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<SqlDatabaseContext>("SqlDatabase")
            .AddCheck<VectorDatabaseContextHealthCheck>("VectorDatabase");
    }
}