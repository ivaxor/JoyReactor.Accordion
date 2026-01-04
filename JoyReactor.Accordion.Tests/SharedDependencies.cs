using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Media.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.Tests;

public static class SharedDependencies
{
    public static readonly SqlDatabaseContext SqlDatabaseContext;
    public static readonly GraphQLHttpClient GraphQLHttpClient;

    public static readonly ApiClient ApiClient;
    public static readonly TagClient TagClient;
    public static readonly PostClient PostClient;
    public static readonly PostCrawler PostParser;
    public static readonly ImageReducer ImageReducer;
    public static readonly ImageDownloader ImageDownloader;

    static SharedDependencies()
    {
        var sqlDatabaseContextOptions = new DbContextOptionsBuilder<SqlDatabaseContext>()
            .UseInMemoryDatabase(nameof(SqlDatabaseContext))
            .Options;
        SqlDatabaseContext = new SqlDatabaseContext(sqlDatabaseContextOptions);

        var apiClientSettingsOptions = Options.Create(new ApiClientSettings()
        {
            GraphQlEndpointUrl = "https://api.joyreactor.cc/graphql",
            MaxRetryAttempts = 10,
            SubsequentCallDelay = TimeSpan.FromSeconds(2.5),
        });
        GraphQLHttpClient = new GraphQLHttpClient(apiClientSettingsOptions.Value.GraphQlEndpointUrl, new SystemTextJsonSerializer());

        ApiClient = new ApiClient(
            GraphQLHttpClient,
            apiClientSettingsOptions,
            NullLogger<ApiClient>.Instance);

        TagClient = new TagClient(
            ApiClient,
            NullLogger<TagClient>.Instance);

        PostClient = new PostClient(ApiClient);

        PostParser = new PostCrawler(SqlDatabaseContext);

        var imageDownloaderHttpClient = new HttpClient();
        var imageSettingsOptions = Options.Create(new ImageSettings()
        {
            CdnDomainNames = [
                "https://img0.joyreactor.cc",
                "https://img1.joyreactor.cc",
                "https://img2.joyreactor.cc",
                "https://img10.joyreactor.cc",
            ],
            ResizedSize = 224,
        });

        ImageReducer = new ImageReducer(imageSettingsOptions);

        ImageDownloader = new ImageDownloader(
            imageDownloaderHttpClient,
            ImageReducer,
            imageSettingsOptions);
    }
}