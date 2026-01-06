using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media.Images;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.Logic.Parsers;
using JoyReactor.Accordion.WebAPI.BackgroudServices;
using JoyReactor.Accordion.WebAPI.Controllers;
using JoyReactor.Accordion.WebAPI.Extensions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using System.Reflection;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
builder.AddLogging();
builder.AddOptionsFromConfiguration();
builder.AddGraphQlClient();
builder.AddDatabases();
builder.AddInferenceSession();
builder.AddRateLimiter();
builder.AddHealthChecks();

var userAgent = $"JoyReactor.Accordion/{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)} (Bot; +https://github.com/ivaxor/JoyReactor.Accordion)";
var socketsHttpHandler = new SocketsHttpHandler()
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    AllowAutoRedirect = true,
    MaxAutomaticRedirections = 3,
};

builder.Services.AddHttpClient();
builder.Services
    .AddHttpClient<IImageDownloader, ImageDownloader>(httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        httpClient.DefaultRequestHeaders.Add("Referer", "https://joyreactor.cc");
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
    })
    .ConfigurePrimaryHttpMessageHandler(() => socketsHttpHandler);
builder.Services
    .AddHttpClient<SearchPicturesController>(httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
    })
    .ConfigurePrimaryHttpMessageHandler(() => socketsHttpHandler);

builder.Services.AddSingleton<IApiClient, ApiClient>();
builder.Services.AddSingleton<ITagClient, TagClient>();
builder.Services.AddScoped<ITagCrawler, TagCrawler>();
builder.Services.AddSingleton<IPostClient, PostClient>();
builder.Services.AddScoped<IPostParser, PostParser>();
builder.Services.AddSingleton<IImageReducer, ImageReducer>();
builder.Services.AddSingleton<IImageDownloader, ImageDownloader>();
builder.Services.AddSingleton<IOnnxVectorConverter, OnnxVectorConverter>();
builder.Services.AddSingleton<IVectorDatabaseContext, VectorDatabaseContext>();

builder.Services.AddHostedService<MainTagsCrawler>();
builder.Services.AddHostedService<TagSubTagsCrawler>();
builder.Services.AddHostedService<TagInnnerRangeCrawler>();
builder.Services.AddHostedService<TagOuterRangeCrawler>();
builder.Services.AddHostedService<PicturesWithoutVectorCrawler>();
builder.Services.AddHostedService<CrawlerTaskHandler>();
//builder.Services.AddHostedService<TopWeekPostsCrawler>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();