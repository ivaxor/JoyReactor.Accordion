using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class TopWeekPostsCrawler(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<TopWeekPostsCrawler> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => false;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        var startYear = 2009;
        var startWeek = 12;

        var endYear = DateTime.UtcNow.Date.Year;
        var endYearWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) - 1;

        if (endYearWeek < 0)
        {
            endYear -= 1;
            endYear = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(new DateTime(endYear, 12, 31), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        var nsfw = false;

        for (var year = startYear; year <= endYear; year++)
        {
            var yearLastWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(new DateTime(year, 12, 31), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            for (var week = (year == startYear ? startWeek : 0); week <= (year == endYear ? endYearWeek : yearLastWeek); week++)
            {
                logger.LogInformation("Crawling {Week} week of {Year} year top {PostType}.", week, year, nsfw ? "nsfw posts" : "posts");
                await CrawlAsync(year, week, nsfw, cancellationToken);
            }
        }
    }

    protected async Task CrawlAsync(int year, int week, bool nsfw, CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
        var postClient = serviceScope.ServiceProvider.GetRequiredService<IPostClient>();
        var postParser = serviceScope.ServiceProvider.GetRequiredService<IPostParser>();

        var posts = await postClient.GetWeekTopPostsAsync(year, week, nsfw, cancellationToken);
        logger.LogInformation("Found {PostCount} top {Week} week of {Year} year {PostType}.", posts.Length, week, year, nsfw ? "nsfw posts" : "posts");

        await postParser.ParseAsync(posts, cancellationToken);
    }
}