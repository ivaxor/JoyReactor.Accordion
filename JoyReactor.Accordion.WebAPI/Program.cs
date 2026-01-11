using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Crawlers;
using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.Logic.Media;
using JoyReactor.Accordion.Logic.Onnx;
using JoyReactor.Accordion.Logic.Parsers;
using JoyReactor.Accordion.WebAPI.BackgroudServices;
using JoyReactor.Accordion.WebAPI.Controllers;
using JoyReactor.Accordion.WebAPI.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
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

var userAgent = $"JoyReactor.Accordion/{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)} (Bot; +https://github.com/ivaxor/joyreactor-accordion)";

builder.Services.AddHttpClient();
builder.Services
    .AddHttpClient<IMediaDownloader, MediaDownloader>(httpClient =>
    {
        httpClient.DefaultRequestHeaders.Add("Referer", "https://joyreactor.cc");
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
    });
builder.Services
    .AddHttpClient<SearchMediaController>(httpClient =>
    {
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
    });

builder.Services.AddSingleton<IApiClient, ApiClient>();
builder.Services.AddSingleton<ITagClient, TagClient>();
builder.Services.AddScoped<ITagCrawler, TagCrawler>();
builder.Services.AddSingleton<IPostClient, PostClient>();
builder.Services.AddScoped<IPostParser, PostParser>();
builder.Services.AddSingleton<IMediaReducer, MediaReducer>();
builder.Services.AddSingleton<IOnnxVectorConverter, OnnxVectorConverter>();
builder.Services.AddSingleton<IVectorDatabaseContext, VectorDatabaseContext>();

builder.Services.AddHostedService<MainTagsCrawler>();
builder.Services.AddHostedService<TagInnnerRangeCrawler>();
builder.Services.AddHostedService<TagOuterRangeCrawler>();
builder.Services.AddHostedService<TagSubTagsCrawler>();
builder.Services.AddHostedService<MediaToVectorConverter>();
builder.Services.AddHostedService<CrawlerTaskHandler>();
builder.Services.AddHostedService<ParsedPostAttributePictureImageTypeFixer>();
builder.Services.AddHostedService<VectorNormalizator>();
builder.Services.AddHostedService<VectorPostDuplicateRemover>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    // TODO: Update with settings for prod
    options.AddDefaultPolicy(builder => builder
        .AllowAnyHeader()
        .AllowAnyOrigin()
        .AllowAnyMethod());
});

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
}
else
{
    //app.UseHttpsRedirection();
    //app.UseHsts();
    app.UseRateLimiter();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();