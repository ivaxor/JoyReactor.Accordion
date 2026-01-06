using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.WebAPI.BackgroudServices;
using JoyReactor.Accordion.WebAPI.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Serilog;

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
        builder.Services.Configure<ImageSettings>(builder.Configuration.GetSection(nameof(ImageSettings)));
        builder.Services.Configure<OnnxSettings>(builder.Configuration.GetSection(nameof(OnnxSettings)));
        builder.Services.Configure<PostgreSqlSettings>(builder.Configuration.GetSection(nameof(PostgreSqlSettings)));
        builder.Services.Configure<QdrantSettings>(builder.Configuration.GetSection(nameof(QdrantSettings)));
        builder.Services.Configure<BackgroundServiceSettings>(builder.Configuration.GetSection(nameof(BackgroundServiceSettings)));
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
#if !DEBUG
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-Real-Ip"].FirstOrDefault() ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? httpContext.Connection.RemoteIpAddress.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
            }));
});
#endif
    }

    public static void AddHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<SqlDatabaseContext>(nameof(SqlDatabaseContext))
            .AddCheck<VectorDatabaseContextHealthCheck>(nameof(VectorDatabaseContext));
    }
}