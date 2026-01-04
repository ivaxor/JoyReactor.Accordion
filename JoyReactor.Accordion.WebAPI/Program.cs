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
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
builder.AddOptionsFromConfiguration();
builder.AddGraphQlClient();
builder.AddDatabases();
builder.AddInferenceSession();
builder.AddRateLimiter();
builder.AddHealthChecks();

builder.Services.AddHttpClient();
builder.Services
    .AddHttpClient<IImageDownloader, ImageDownloader>(httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        httpClient.DefaultRequestHeaders.Add("Referer", "https://joyreactor.cc");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 3,
    });
builder.Services
    .AddHttpClient<SearchPicturesController>(httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "JoyReactor.Accordion (Bot; +https://joyreactor.cc)");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 3,
    });

builder.Services.AddSingleton<IApiClient, ApiClient>();
builder.Services.AddSingleton<ITagClient, TagClient>();
builder.Services.AddScoped<ITagCrawler, TagCrawler>();
builder.Services.AddSingleton<IPostClient, PostClient>();
builder.Services.AddScoped<IPostParser, PostParser>();
builder.Services.AddSingleton<IImageReducer, ImageReducer>();
builder.Services.AddSingleton<IImageDownloader, ImageDownloader>();
builder.Services.AddSingleton<IOnnxVectorConverter, OnnxVectorConverter>();
builder.Services.AddSingleton<IVectorDatabaseContext, VectorDatabaseContext>();

builder.Services.AddScopedHostedService<MainTagsCrawler>();
builder.Services.AddScopedHostedService<TagSubTagsCrawler>();
builder.Services.AddScopedHostedService<TagInnnerRangeCrawler>();
builder.Services.AddScopedHostedService<TagOuterRangeCrawler>();
builder.Services.AddScopedHostedService<PicturesWithoutVectorCrawler>();
//builder.Services.AddScopedHostedService<TopWeekPostsCrawler>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

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
